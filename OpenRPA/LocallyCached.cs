﻿using OpenRPA.Interfaces;
using OpenRPA.Interfaces.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA
{
    public class LocallyCached : apibase
    {
        private static object savelock = new object();
        public async Task Save<T>(bool skipOnline = false) where T : apibase
        {
            try
            {
                _backingFieldValues["_disabledirty"] = true;
                var entity = (T)Convert.ChangeType(this, typeof(T));
                if (!global.isConnected )
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_id))
                        {
                            _id = Guid.NewGuid().ToString();
                            isLocalOnly = true;
                            // collection.Insert(entity);
                        }
                        else
                        {
                            entity._version++; // Add one to avoid watch update
                            // collection.Update(entity);
                            entity._modified = DateTime.Now;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                } 
                else if (string.IsNullOrEmpty(_id))
                {
                    isDirty = true;
                    _id = Guid.NewGuid().ToString();
                }
                    string collectionname = "openrpa";
                if (_type == "workflowinstance") collectionname = "openrpa_instances";
                if (_type == "workitemqueue") collectionname = "mq";
                if (_type == "workitem") collectionname = "workitems";
                
                if (_type == "workflowinstance" && Config.local.skip_online_state) {
                    skipOnline = true;
                    isDirty = false;
                }
                if (global.isConnected && !skipOnline)
                {
                    if (string.IsNullOrEmpty(_id) || isLocalOnly == true)
                    {
                        T result = default(T);
                        try
                        {
                            result = await global.webSocketClient.InsertOne(collectionname, 0, false, entity, "", "");
                        }
                        catch (Exception ex)
                        {
                            if(ex.Message.Contains("E11000 duplicate key error"))
                            {
                                result = await global.webSocketClient.InsertOrUpdateOne(collectionname, 0, false, null, entity, "", "");
                            } else
                            {
                                throw;
                            }
                        }
                        EnumerableExtensions.CopyPropertiesTo(result, entity, true);
                        isLocalOnly = false;
                        isDirty = false;
                        Log.Verbose("Inserted to openflow and returned as version " + entity._version + " " + entity._type + " " + entity.name);
                    }
                    else
                    {
                        if (entity.isDirty)
                        {
                            entity._version++; // Add one to avoid watch update
                            try
                            {
                                isDirty = false;
                                var result = await global.webSocketClient.InsertOrUpdateOne(collectionname, 0, false, null, entity, "", "");
                                if (result != null)
                                {
                                    if(result._id != entity._id && !string.IsNullOrEmpty(entity._id))
                                    {
                                        await StorageProvider.Delete<T>(entity._id);
                                    }
                                    if(_type != "workflowinstance")
                                    {
                                        EnumerableExtensions.CopyPropertiesTo(result, entity, true);
                                    } else
                                    {
                                        _acl = result._acl;
                                        _modified = result._modified;
                                        _modifiedby = result._modifiedby;
                                        _modifiedbyid = result._modifiedbyid;
                                        _created = result._created;
                                        _createdby = result._createdby;
                                        _createdbyid = result._createdbyid;
                                        _version = result._version;
                                    }
                                    // isDirty = false;
                                    Log.Verbose("Updated in openflow and returned as version " + entity._version + " " + entity._type + " " + entity.name);
                                } else { isDirty = true; }
                            }
                            catch (Exception) 
                            {
                                //Log.Debug("Failed saving " + entity._type + " " + entity._id + " will be updated at next sync or save");
                                throw;
                            }
                        }
                    }
                }
                if (System.Threading.Monitor.TryEnter(savelock, Config.local.thread_lock_timeout_seconds * 1000))
                {
                    try
                    {
                        if(!string.IsNullOrEmpty(_id))
                        {
                            var exists = await StorageProvider.FindById<T>(_id);
                            if (exists != null) { await StorageProvider.Update(entity); Log.Verbose("Updated in local db as version " + entity._version + " " + entity._type + " " + entity.name); }
                            if (exists == null) { await StorageProvider.Insert(entity); Log.Verbose("Inserted in local db as version  " + entity._version + " " + entity._type + " " + entity.name); }
                        }
                    }
                    finally
                    {
                        System.Threading.Monitor.Exit(savelock);
                    }
                }
                else { 
                    if(Config.local.thread_exit_on_lock_timeout)
                    {
                        Log.Error("Locally Cached savelock");
                        System.Environment.Exit(1);
                    }
                    throw new LockNotReceivedException("Locally Cached savelock"); 
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _backingFieldValues.Remove("_disabledirty");
            }
        }
        public async Task Delete<T>() where T : apibase
        {
            var entity = (T)Convert.ChangeType(this, typeof(T));
            if (!global.isConnected)
            {
                try
                {
                    isDeleted = true;
                    isDirty = true;
                    if (System.Threading.Monitor.TryEnter(savelock, Config.local.thread_lock_timeout_seconds * 1000))
                    {
                        try
                        {
                            var exists = StorageProvider.FindById<T>(_id);
                            if (string.IsNullOrEmpty(Config.local.wsurl))
                            {
                                if (exists != null) { await StorageProvider.Delete<T>(entity._id); Log.Verbose("Deleted from local " + entity._type + " " + entity.name); }
                            } else
                            {
                                if (exists != null) { await StorageProvider.Update(entity); Log.Verbose("Updated in local db as version " + entity._version + " " + entity._type + " " + entity.name); }
                                if (exists == null) { await StorageProvider.Insert(entity); Log.Verbose("Inserted in local db as version  " + entity._version + " " + entity._type + " " + entity.name); }
                            }
                        }
                        finally
                        {
                            System.Threading.Monitor.Exit(savelock);
                        }
                    }
                    else {
                        if (Config.local.thread_exit_on_lock_timeout)
                        {
                            Log.Error("Locally Cached savelock");
                            System.Environment.Exit(1);
                        }
                        throw new LockNotReceivedException("Locally Cached savelock"); 
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            else
            {
                string collectionname = "openrpa";
                if (_type == "workflowinstance") collectionname = "openrpa_instances";

                await global.webSocketClient.DeleteOne(collectionname, entity._id, "", "");
                Log.Verbose("Deleted in openflow and as version " + entity._version + " " + entity._type + " " + entity.name);
                if (System.Threading.Monitor.TryEnter(savelock, Config.local.thread_lock_timeout_seconds * 1000))
                {
                    try
                    {
                        var exists = await StorageProvider.FindById<T>(_id);
                        if (exists != null) { await StorageProvider.Delete<T>(entity._id); Log.Verbose("Deleted in local db as version " + entity._version + " " + entity._type + " " + entity.name); }
                    }
                    finally
                    {
                        System.Threading.Monitor.Exit(savelock);
                    }
                }
                else {
                    if (Config.local.thread_exit_on_lock_timeout)
                    {
                        Log.Error("Locally Cached savelock");
                        System.Environment.Exit(1);
                    }
                    throw new LockNotReceivedException("Locally Cached savelock"); 
                }
            }
        }
    }
}
