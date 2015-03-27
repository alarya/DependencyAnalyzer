:run.bat
:
:runs both the servers & Client
cd "Server/bin/Debug"
start Server.exe http://localhost:8080/MessageService http://localhost:8081/MessageService
 
cd "../../../Server1/bin/Debug/"
start Server1.exe http://localhost:8081/MessageService http://localhost:8080/MessageService

cd "../../../Client_GUI/Client_GUI/bin/Debug"
start Client_GUI.exe
start Client_GUI.exe
cd "../../../../"