SET PYHOME=%1%

%PYHOME%/python.exe pip.pyz install --no-index setuptools-68.0.0-py3-none-any.whl
%PYHOME%/python.exe pip.pyz install --no-index pip-23.1.2-py3-none-any.whl

%PYHOME%/Scripts/pip install --no-index urllib3-1.26.16-py2.py3-none-any.whl

%PYHOME%/Scripts/pip install --no-index attrs-19.3.0-py2.py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index sortedcontainers-2.4.0-py2.py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index async_generator-1.10-py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index idna-3.4-py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index outcome-1.2.0-py2.py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index sniffio-1.3.0-py3-none-any.whl

%PYHOME%/Scripts/pip install --no-index pycparser-2.21-py2.py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index cffi-1.15.1-cp37-cp37m-win_amd64.whl

%PYHOME%/Scripts/pip install --no-index trio-0.17.0-py3-none-any.whl

%PYHOME%/Scripts/pip install --no-index exceptiongroup-1.1.2-py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index h11-0.12.0-py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index wsproto-1.2.0-py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index trio_websocket-0.10.3-py3-none-any.whl

%PYHOME%/Scripts/pip install --no-index certifi-2023.5.7-py3-none-any.whl

%PYHOME%/Scripts/pip install --no-index PySocks-1.7.1-py3-none-any.whl

%PYHOME%/Scripts/pip install --no-index selenium-4.10.0-py3-none-any.whl

%PYHOME%/Scripts/pip install --no-index uniplore_rpatest-1.0.0-py3-none-any.whl
%PYHOME%/Scripts/pip install --no-index uniplore_test_baseui-1.0.0-py3-none-any.whl

