using Miracle.FileZilla.Api;
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
    //TODO: //Проверить пути алиасов-групп и папок юзеров
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
        static private void CreateFTPUser(XmlNode attr)
        {
            int Counter = 0;
            string Name = null, Group = null, Password = null, Login = null;
            ushort Limit = 1024;
            foreach (XmlNode childnode in attr.ChildNodes)
            {
                switch (childnode.Name)
                {
                    case "Name":
                        Name = childnode.InnerText;
                        Counter++;
                        break;
                    case "Group":
                        Group = childnode.InnerText;
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
            if (Counter != 5)
            {
                throw new InvalidXMLExeption();
            }
            SpeedLimit SpeedLimit = new SpeedLimit 
            {
                ConstantSpeedLimit = Limit,
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
                    Comment = Login,
                    UserName = Name,
                    SharedFolders = new List<SharedFolder>()
                    {
                        new SharedFolder()
                        {
                            Directory = @"D:\FTP_accounts\{Name}",
                            AccessRights = AccessRights.DirList | AccessRights.DirSubdirs | AccessRights.FileRead | AccessRights.FileWrite | AccessRights.IsHome| AccessRights.AutoCreate
                        }
                    },
                    DownloadSpeedLimit = SpeedLimit,
                    UploadSpeedLimit = SpeedLimit
                };
                CreatedUser.AllowedIPs.Add("192.168.10.101");
                CreatedUser.AssignPassword(Password, fileZillaApi.ProtocolVersion);
                Settings.Users.Add(CreatedUser);
                fileZillaApi.SetAccountSettings(Settings);
                if (Group != "No Groop")
                {
                    EditUserGroup(Name, Group);
                }
            }
            catch (Exception ex)
            {
                Log("CreateUser", ex.Message);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DateTime.Now + "  Creating User with name - {Name}");
            Console.ResetColor();
        }
        static private void RemoveUser(XmlNode attr)
        {
            try
            {
                string Name = null;
                if (attr.ChildNodes.Count == 1 && attr.ChildNodes[0].Name == "Name")
                {
                    Name = attr.ChildNodes[0].InnerText;
                }
                else
                {
                    throw new InvalidXMLExeption();
                }
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var settings = fileZillaApi.GetAccountSettings();
                var DeletMe = settings.Users.Find((User) => Equals(User.UserName, Name));
                settings.Users.Remove(DeletMe);
                fileZillaApi.SetAccountSettings(settings);
                Directory.Delete(@"D:\FTP_accounts\{UserName}", true);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now + "  Removing User with name - {Name}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Log("RemoveUser", ex.Message);
            }
        }
        static private void BanIp(XmlNode attr)
        {
            try
            {
                string Name = null;
                if (attr.ChildNodes.Count == 1 && attr.ChildNodes[0].Name == "IP")
                {
                    Name = attr.ChildNodes[0].InnerText;
                }
                else
                {
                    throw new InvalidXMLExeption();
                }
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var ConnectID = fileZillaApi.GetConnections().Find((x) => Equals(x.Ip, Name)).ConnectionId;
                fileZillaApi.BanIp(ConnectID);
                StreamWriter writer = new StreamWriter("\\BannedIp.Txt", true, System.Text.Encoding.UTF8);
                writer.WriteLine(Name);
                writer.Flush();
                writer.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now + "  Ban IP {Name}");
                Console.ResetColor();
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
        static private void EditUserSpeed(XmlNode attr)
        {
            try
            {
                string Name = null;
                ushort Limit = 1024;
                int Counter = 0;
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
                SpeedLimit SpeedLimit = new SpeedLimit
                {
                    ConstantSpeedLimit = Limit,
                    SpeedLimitType = SpeedLimitType.ConstantSpeedLimit,
                };
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var settings = fileZillaApi.GetAccountSettings();
                var EditMe = settings.Users.Find((User) => Equals(User.UserName, Name));
                EditMe.UploadSpeedLimit = SpeedLimit;
                EditMe.DownloadSpeedLimit = SpeedLimit;
                fileZillaApi.SetAccountSettings(settings);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(DateTime.Now + "  Editing speed for user {Name} : new speed - {Limit} Kbs");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Log("BanIP", ex.Message);
            }
        }
        static void UnWrapEditGroup(XmlNode attr)
        {
            string Name = null, GroupName = null;
            int Counter = 0;
            foreach (XmlNode childnode in attr.ChildNodes)
            {
                switch (childnode.Name)
                {
                    case "Name":
                        Name = childnode.InnerText;
                        Counter++;
                        break;
                    case "GrupName":
                        GroupName = childnode.InnerText;
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
            Task.Run(() => EditUserGroup(Name, GroupName));
        }
        static private void EditUserGroup(string UserName, string Group)
        {
            try
            {
                var fileZillaApi = new FileZillaApi(IPAddress.Parse(FileZillaIp), FileZillaPort);
                fileZillaApi.Connect(FileZillaPass);
                var settings = fileZillaApi.GetAccountSettings();
                var EditMe = settings.Users.Find((User) => Equals(User.UserName, UserName));
                EditMe.GroupName = Group;
                RemoveAlias(UserName, "Films");
                RemoveAlias(UserName, "Soft");
                switch (Group)
                {
                    case "Films":
                        AddAlias(UserName, @"\D\Films", "Films", new Permissions());
                        break;
                    case "Soft":
                        AddAlias(UserName, @"\D\Soft", "Soft", new Permissions());
                        break;
                    case "Films + Soft":
                        AddAlias(UserName, @"\D\Films", "Films", new Permissions());
                        AddAlias(UserName, @"\D\Soft", "Soft", new Permissions());
                        break;
                }
            }
            catch (Exception ex)
            {
                Log("EditUser", ex.Message);
            }

        }
      static private void UnWrapXMLAlias(XmlNode attr)
        {
            string Name = null, Path = null, AliasName = null;
            Permissions permissions = new Permissions();
            int Counter = 0;
            foreach (XmlNode childnode in attr.ChildNodes)
            {
                switch (childnode.Name)
                {
                    case "Name":
                        Name = childnode.InnerText;
                        Counter++;
                        break;
                    case "Path":
                        Path = childnode.InnerText;
                        Counter++;
                        break;
                    case "AliasName":
                        AliasName = childnode.InnerText;
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
            Task.Run(() => AddAlias(Name, Path, AliasName, permissions));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(DateTime.Now + "  Editing User with name - {Name} to group {Login}");
            Console.ResetColor();
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(DateTime.Now + "  Addng alias \"{Login}\" to user {Name}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Log("AddAlias", ex.Message);
            }
        }
        static void EditAliasPermissions(XmlNode attr)
        {
            try
            {
                Permissions Perms = new Permissions();
                int Counter = 0;
                string UserName = null,AliasName = null;
                foreach (XmlNode childnode in attr.ChildNodes)
                {
                    switch (childnode.Name)
                    {
                        case "Name":
                            UserName = childnode.InnerText;
                            Counter++;
                            break;
                        case "AliasName":
                            AliasName = childnode.InnerText;
                            Counter++;
                            break;
                        case "Dcreate":
                            Perms.Dcreate = bool.Parse(childnode.InnerText);
                            Counter++;
                            break;
                        case "DDelete":
                            Perms.DDelete = bool.Parse(childnode.InnerText);
                            Counter++;
                            break;
                        case "DList":
                            Perms.DList = bool.Parse(childnode.InnerText);
                            Counter++;
                            break;
                        case "FAppend":
                            Perms.FAppend = bool.Parse(childnode.InnerText);
                            Counter++;
                            break;
                        case "FDelete":
                            Perms.FDelete = bool.Parse(childnode.InnerText);
                            Counter++;
                            break;
                        case "FRead":
                            Perms.FRead = bool.Parse(childnode.InnerText);
                            Counter++;
                            break;
                        case "FWrite":
                            Perms.FWrite = bool.Parse(childnode.InnerText);
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(DateTime.Now + "  Creating User with name - {Name}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Log("EditAliasPerms", ex.Message);
            }
        }
        static void UnWrapXMLRemoveAlias(XmlNode attr)
        {
            string Name = null, AliasName = null;
            int Counter = 0;
            foreach (XmlNode childnode in attr.ChildNodes)
            {
                switch (childnode.Name)
                {
                    case "Name":
                        Name = childnode.InnerText;
                        Counter++;
                        break;
                    case "AliasName":
                        AliasName = childnode.InnerText;
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
            Task.Run(() => RemoveAlias(Name, AliasName));
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
                if (Editme != null)
                {
                    User.SharedFolders.Remove(Editme);
                    fileZillaApi.SetAccountSettings(settings);
                }
                else
                {
                    if (AliasName != "Soft" && AliasName != "Films")
                    {
                        throw new Exception("Cant find this group!");
                    }
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(DateTime.Now + "  Creating User with name - {Name}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Log("RemoveAlias", ex.Message);
            }
        }
        static void Main(string[] args)
        {
            string MethodName = "Main";
            byte[] data;
            Console.WriteLine("Start");
            bool ExitTrigger = true;
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), ServerPort);
                Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(ipPoint);
                Console.WriteLine(DateTime.Now  + "  Bilding");
                listenSocket.Listen(100);
                Console.WriteLine(DateTime.Now +  "  Listening");
                Socket handler = listenSocket.Accept();
                Console.WriteLine(DateTime.Now +"  Ready!");
                while (ExitTrigger)
                {
                    data = new byte[2560];
                    handler.Receive(data);
                    XmlDocument XMLDoc = new XmlDocument();
                    string xml = Encoding.UTF8.GetString(data);
                    XMLDoc.LoadXml(xml);
                    XmlElement xRoot = XMLDoc.DocumentElement;
                    XmlNode attr = xRoot.Attributes.GetNamedItem("Method");
                    MethodName = attr.Value;
                    Permissions permissions = new Permissions();
                    switch (MethodName)
                    {
                        case "Shutdown":
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            StreamWriter writer = new StreamWriter("\\ErrorLog.txt", true, System.Text.Encoding.UTF8);
                            writer.WriteLine(DateTime.Now + "  Closing");
                            writer.Flush();
                            writer.Close();
                            ExitTrigger = false;
                            break;

                        case "Restart":
                            throw new Exception("Restatring");

                        case "CreateUser":
                            Task.Run(() => CreateFTPUser(attr));
                            break;

                        case "RemoveUser":            
                            Task.Run(() => RemoveUser(attr));
                            break;

                        case "BanIP":
                            Task.Run(() => BanIp(attr));
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
                            Console.WriteLine(DateTime.Now + "  Getting banned IP list");
                            break;

                        case "EditSpeed":
                            Task.Run(() => EditUserSpeed(attr));
                            break;

                        case "AddAlias":
                            Task.Run(() => UnWrapXMLAlias(attr));
                            break;

                        case "EditAliasPerms":
                            Task.Run(() => EditAliasPermissions(attr));
                            break;

                        case "RemoveAlias":
                            Task.Run(() => UnWrapXMLRemoveAlias(attr));
                            break;

                        case "EditGroup":
                            Task.Run(() => UnWrapEditGroup(attr));
                            break;

                        default:
                            throw new InvalidXMLExeption();
                    }
                }
            }
            catch(Exception ex)
            {
                Log(MethodName,ex.Message);
            }
        }
    }   
}
