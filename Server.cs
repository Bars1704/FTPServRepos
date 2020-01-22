﻿using Miracle.FileZilla.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Client_ServerTest01
{
    public class InvalidXMLExeption : Exception
    {
        public new string Message = "Invalid XML";
        public InvalidXMLExeption()
        {

        }
    }
    class Server
    {
        const int ServerPort = 2536;
        const int FileZillaPort = 14147;
        const string FileZillaPass = "123456789";
        const string FileZillaIp = "127.0.0.1";
        static private void Log(string MethodName, string Error)
        {
            StreamWriter writer = new StreamWriter("\\ErrorLog.txt", true, System.Text.Encoding.UTF8);
            writer.WriteLine(DateTime.Now + " " + MethodName + ":  " + Error);
            writer.Flush();
            writer.Close();
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }

        static private void CreateFTPUser(string UserName, string Password, string UserLogin, ushort SpeedLimit = 10240)
        {
            SpeedLimit Limit = new SpeedLimit 
            {
                ConstantSpeedLimit = SpeedLimit,
                SpeedLimitType = SpeedLimitType.ConstantSpeedLimit,
            };
            try
            {
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var Settings = fileZillaApi.GetAccountSettings();
                Directory.CreateDirectory(@"D:\FTP_accounts\{UserName}");
                var CreatedUser = new User
                {
                    Comment = UserLogin,
                    UserName = UserName,
                    SharedFolders = new List<SharedFolder>()
                    {
                        new SharedFolder()
                        {
                            Directory = @"D:\FTP_accounts\{Name}",
                            AccessRights = AccessRights.DirList | AccessRights.DirSubdirs | AccessRights.FileRead | AccessRights.FileWrite | AccessRights.IsHome| AccessRights.AutoCreate
                        }
                    },
                    DownloadSpeedLimit = Limit,
                    UploadSpeedLimit = Limit
                };
                CreatedUser.AllowedIPs.Add("192.168.10.101");
                CreatedUser.AssignPassword(Password, fileZillaApi.ProtocolVersion);
                Settings.Users.Add(CreatedUser);
                fileZillaApi.SetAccountSettings(Settings);
            }
            catch (Exception ex)
            {
                Log("CreateUser", ex.Message);
            }
        }
        static private void RemoveUser(string UserName)
        {
            try
            {
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var settings = fileZillaApi.GetAccountSettings();
                var DeletMe = settings.Users.Find((User) => Equals(User.UserName, UserName));
                settings.Users.Remove(DeletMe);
                fileZillaApi.SetAccountSettings(settings);
                Directory.Delete(@"D:\FTP_accounts\{UserName}", true);
            }
            catch (Exception ex)
            {
                Log("RemoveUser", ex.Message);
            }
        }
        static private void BanIp(string Ip)
        {
            try
            {
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var ConnectID = fileZillaApi.GetConnections().Find((x) => Equals(x.Ip, Ip)).ConnectionId;
                fileZillaApi.BanIp(ConnectID);
                StreamWriter writer = new StreamWriter("\\BannedIp.Txt", true, System.Text.Encoding.UTF8);
                writer.WriteLine(Ip);
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                Log("BanIP", ex.Message);
            }
        }

        static private List<string> GetBannedIps()
        {
            try
            {
                StreamReader reader = new StreamReader("\\BannedIp.Txt", System.Text.Encoding.UTF8);
                reader.Close();
                var Ip = new List<string>();
                string currentIP;
                while ((currentIP = reader.ReadLine()) != null)
                {
                    Ip.Add(currentIP);
                }
                return Ip;
            }
            catch
            {
                return null;
            }
        }

        static private void EditUserSpeed(string UserName, ushort SpeedLimit)
        {
            try
            {
                SpeedLimit limit = new SpeedLimit
                {
                    ConstantSpeedLimit = SpeedLimit,
                    SpeedLimitType = SpeedLimitType.ConstantSpeedLimit,
                };
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var settings = fileZillaApi.GetAccountSettings();
                var EditMe = settings.Users.Find((User) => Equals(User.UserName, UserName));
                EditMe.UploadSpeedLimit = limit;
                EditMe.DownloadSpeedLimit = limit;
                fileZillaApi.SetAccountSettings(settings);
            }
            catch (Exception ex)
            {
                Log("BanIP", ex.Message);
            }
        }

      static  private void AddAlias(string UserName, string Path, string AliasName, Permissions Perms)
        {
            try
            {
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var settings = fileZillaApi.GetAccountSettings();
                var EditMe = settings.Users.Find((User) => Equals(User.UserName, UserName));
                SharedFolder AddMe = new SharedFolder { Directory = Path , Aliases = new List<string> { AliasName } };
                AddMe.AccessRights = AccessRights.DirSubdirs;
                if (Perms.Dcreate)
                {
                    AddMe.AccessRights |= AccessRights.DirCreate;
                }
                if (Perms.DDelete)
                {
                    AddMe.AccessRights |= AccessRights.DirDelete;
                }
                if (Perms.DList)
                {
                    AddMe.AccessRights |= AccessRights.DirList;
                }
                if (Perms.FAppend)
                {
                    AddMe.AccessRights |= AccessRights.FileAppend;
                }
                if (Perms.FDelete)
                {
                    AddMe.AccessRights |= AccessRights.FileDelete;
                }
                if (Perms.FRead)
                {
                    AddMe.AccessRights |= AccessRights.FileRead;
                }
                if (Perms.FWrite)
                {
                    AddMe.AccessRights |= AccessRights.FileWrite;
                }
                fileZillaApi.SetAccountSettings(settings);
            }
            catch (Exception ex)
            {
                Log("AddAlias", ex.Message);
            }
        }

       static void EditAliasPermissions(string UserName, string AliasName, Permissions Perms)
        {
            try
            {
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var Settings = fileZillaApi.GetAccountSettings();
                var User = Settings.Users.Find((CurrentUser) => Equals(CurrentUser.UserName, UserName));
                SharedFolder Editme = User.SharedFolders.Find((Folder) => Folder.Aliases.Contains(AliasName));
                Editme.AccessRights = AccessRights.DirSubdirs;
                if (Perms.Dcreate)
                {
                    Editme.AccessRights |= AccessRights.DirCreate;
                }
                if (Perms.DDelete)
                {
                    Editme.AccessRights |= AccessRights.DirDelete;
                }
                if (Perms.DList)
                {
                    Editme.AccessRights |= AccessRights.DirList;
                }
                if (Perms.FAppend)
                {
                    Editme.AccessRights |= AccessRights.FileAppend;
                }
                if (Perms.FDelete)
                {
                    Editme.AccessRights |= AccessRights.FileDelete;
                }
                if (Perms.FRead)
                {
                    Editme.AccessRights |= AccessRights.FileRead;
                }
                if (Perms.FWrite)
                {
                    Editme.AccessRights |= AccessRights.FileWrite;
                }
                fileZillaApi.SetAccountSettings(Settings);
            }
            catch (Exception ex)
            {
                Log("EditAliasPerms", ex.Message);
            }
        }

        static void RemoveAlias(string UserName, string AliasName)
        {
            try
            {
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var settings = fileZillaApi.GetAccountSettings();
                var User = settings.Users.Find((CurrUser) => Equals(CurrUser.UserName, UserName));
                SharedFolder Editme = User.SharedFolders.Find((Folder) => Folder.Aliases.Contains(AliasName));
                User.SharedFolders.Remove(Editme);
                fileZillaApi.SetAccountSettings(settings);
            }
            catch (Exception ex)
            {
                Log("RemoveAlias", ex.Message);
            }
        }
        static void Main(string[] args)
        {
            try
            {
                string MethodName;
                byte[] data;
                Console.WriteLine("Start");
                bool ExitTrigger = true;
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), ServerPort);
                Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(ipPoint);
                Console.WriteLine("Bilding");
                listenSocket.Listen(100);
                Console.WriteLine("Listening");
                Socket handler = listenSocket.Accept();
                Console.WriteLine("Ready!");
                while (ExitTrigger)
                {
                    int Counter = 0;
                    data = new byte[2560];
                    handler.Receive(data);
                    XmlDocument XMLDoc = new XmlDocument();
                    string xml = Encoding.UTF8.GetString(data);
                    XMLDoc.LoadXml(xml);
                    XmlElement xRoot = XMLDoc.DocumentElement;
                    XmlNode attr = xRoot.Attributes.GetNamedItem("Method");
                    MethodName = attr.Value;
                    string Name = null, Password = null, Login = null;
                    Permissions permissions = new Permissions();
                    ushort Limit = 0;
                    switch (MethodName)
                    {
                        case "Shutdown":
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            Console.WriteLine("Closing");
                            ExitTrigger = false;
                            break;

                        case "Restart":
                            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                            Environment.Exit(0);
                            break;

                        case "CreateUser":
                            foreach (XmlNode childnode in attr.ChildNodes)
                            {
                                switch (childnode.Name)
                                {
                                    case "Name":
                                        Name = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "Password":
                                        Password = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "Login":
                                        Login = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "SpeedLimit":
                                        Limit = ushort.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    default:
                                        throw new InvalidXMLExeption();
                                }
                            }
                            if (Counter != 4)
                            {
                                throw new InvalidXMLExeption();
                            }
                            Task.Run(() => CreateFTPUser(Name, Password, Login, Limit));
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Creating User with name - {Name}");
                            Console.ResetColor();
                            break;

                        case "RemoveUser":
                            if (attr.ChildNodes.Count == 1 && attr.ChildNodes[0].Name == "Name")
                            {
                                Name = attr.ChildNodes[0].InnerText;
                            }
                            else
                            {
                                throw new InvalidXMLExeption();
                            }
                            Task.Run(() => RemoveUser(Name));
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Creating User with name - {Name}");
                            Console.ResetColor();
                            break;

                        case "BanIP":
                            if (attr.ChildNodes.Count == 1 && attr.ChildNodes[0].Name == "IP")
                            {
                                Name = attr.ChildNodes[0].InnerText;
                            }
                            else
                            {
                                throw new InvalidXMLExeption();
                            }
                            Task.Run(() => BanIp(Name));
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Creating User with name - {Name}");
                            Console.ResetColor();
                            break;

                        case "GetBannedIP":
                            var currtask = Task.Run(() => GetBannedIps()).Result;
                            XMLDoc = new XmlDocument();
                            xRoot = XMLDoc.DocumentElement;
                            XmlElement rootElement = XMLDoc.CreateElement("IPList");
                            foreach (string ip in currtask)
                            {
                                XmlAttribute Node = XMLDoc.CreateAttribute("Line");
                                XmlElement Element = XMLDoc.CreateElement(ip);
                                Node.AppendChild(Element);
                                XMLDoc.AppendChild(Node);
                            }
                            handler.Send(Encoding.Default.GetBytes(XMLDoc.ToString()));
                            Console.WriteLine("Creating User with name - {Name}");
                            break;

                        case "EditSpeed":
                            foreach (XmlNode childnode in attr.ChildNodes)
                            {
                                switch (childnode.Name)
                                {
                                    case "Name":
                                        Name = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "SpeedLimit":
                                        Limit = ushort.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    default:
                                        throw new InvalidXMLExeption();
                                }
                            }
                            if (Counter != 2)
                            {
                                throw new InvalidXMLExeption();
                            }
                            Task.Run(() => EditUserSpeed(Name, Limit));
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Creating User with name - {Name}");
                            Console.ResetColor();
                            break;

                        case "AddAlias":
                            foreach (XmlNode childnode in attr.ChildNodes)
                            {
                                switch (childnode.Name)
                                {
                                    case "Name":
                                        Name = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "Path":
                                        Password = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "AliasName":
                                        Login = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "Dcreate":
                                        permissions.Dcreate = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "DDelete":
                                        permissions.DDelete = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "DList":
                                        permissions.DList = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "FAppend":
                                        permissions.FAppend = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "FDelete":
                                        permissions.FDelete = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "FRead":
                                        permissions.FRead = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "FWrite":
                                        permissions.FWrite = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    default:
                                        throw new InvalidXMLExeption();
                                }
                            }
                            if (Counter != 10)
                            {
                                throw new InvalidXMLExeption();
                            }
                            Task.Run(() => AddAlias(Name, Password, Login, permissions));
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Creating User with name - {Name}");
                            Console.ResetColor();
                            break;

                        case "EditAliasPerms":
                            foreach (XmlNode childnode in attr.ChildNodes)
                            {
                                switch (childnode.Name)
                                {
                                    case "Name":
                                        Name = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "AliasName":
                                        Login = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "Dcreate":
                                        permissions.Dcreate = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "DDelete":
                                        permissions.DDelete = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "DList":
                                        permissions.DList = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "FAppend":
                                        permissions.FAppend = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "FDelete":
                                        permissions.FDelete = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "FRead":
                                        permissions.FRead = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    case "FWrite":
                                        permissions.FWrite = bool.Parse(childnode.InnerText);
                                        Counter++;
                                        break;
                                    default:
                                        throw new InvalidXMLExeption();
                                }
                            }
                            if (Counter != 9)
                            {
                                throw new InvalidXMLExeption();
                            }
                            Task.Run(() => EditAliasPermissions(Name, Login, permissions));
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Creating User with name - {Name}");
                            Console.ResetColor();
                            break;

                        case "RemoveAlias":
                            foreach (XmlNode childnode in attr.ChildNodes)
                            {
                                switch (childnode.Name)
                                {
                                    case "Name":
                                        Name = childnode.InnerText;
                                        Counter++;
                                        break;
                                    case "AliasName":
                                        Login = childnode.InnerText;
                                        Counter++;
                                        break;
                                    default:
                                        throw new InvalidXMLExeption();
                                }
                            }
                            if (Counter != 2)
                            {
                                throw new InvalidXMLExeption();
                            }
                            Task.Run(() => RemoveAlias(Name, Login));
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Creating User with name - {Name}");
                            Console.ResetColor();
                            break;
                        default:
                            throw new InvalidXMLExeption();
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
    }   
}
