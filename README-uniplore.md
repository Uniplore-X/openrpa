### 配置修改
在OpenRPA中增加配置项`settings.json`
```json
{
  "uniplore_properties": {
    "enable": true,
    "uni_base_url": "",
    "uni_login_path": "/uniplore-va/rpa/openRpaLogin",
    "uni_add_token_request_path": "/uniplore-va/rpa/openRpaAddTokenRequest",
    "uni_get_token_request_path": "/uniplore-va/rpa/openRpaGetTokenRequest"
  }
}
```
- `enable` 是否启用，若为false将使用OpenRPA默认的路径（可直接与OpenFlow进行连接）。默认`true`
- `base_url` 基础地址，若为空、则从wsurl里获取（不含path路径）。默认值：`""`
- `login_path` 浏览器登录地址路径，与base_url拼接生成。默认值：`"/uniplore-va/rpa/openRpaLogin"`
- `add_token_request_path` 添加token接口路径，与base_url拼接生成。默认值：`"/uniplore-va/rpa/openRpaAddTokenRequest"`
- `get_token_request_path` 获取token接口路径，与base_url拼接生成。默认值：`"/uniplore-va/rpa/openRpaGetTokenRequest"`

修改代码：
- `OpenRPA/RobotInstance.cs`
- `OpenRPA.Interfaces/Config.cs`
- `OpenRPA.Interfaces/IWebSocketClient.cs`
- `OpenRPA.Net/SigninMessage.cs`
- `OpenRPA.Net/WebSocketClient.cs`