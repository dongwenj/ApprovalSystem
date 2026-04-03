import * as signalR from '@microsoft/signalr';

// 這裡的 URL 要對應你後端 Program.cs 裡 app.MapHub<LogHub>("/hubs/log") 的路徑
const isDev = import.meta.env.DEV;
const baseHubURL = isDev ? 'http://localhost:5266' : (import.meta.env.VITE_HUB_URL || ''); 
const HUB_URL = `${baseHubURL}/hubs/monitor`;

export const setupSignalRConnection = (onMessageReceived: (msg: string) => void) => {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect() // 自動重連
    .configureLogging(signalR.LogLevel.Information)
    .build();

  // 監聽後端 Emit 過來的 "ReceiveLog" 事件
  connection.on("ReceiveLog", (message: string) => {
    onMessageReceived(message);
  });

  connection.start()
    .then(() => console.log("SignalR Connected!"))
    .catch(err => console.error("SignalR Connection Error: ", err));

  return connection;
};