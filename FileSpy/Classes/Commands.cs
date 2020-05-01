namespace FileSpy.Classes
{
    static class Commands
    { 
        public static int Disconnect { get; private set; } = -1;
        public static int Login { get; private set; } = 1;
        public static int AcceptLogin { get; private set; } = 2;

        public static int GetList { get; private set; } = 3;
        public static int List { get; private set; } = 4;
        public static int AddUser { get; private set; } = 5;
        public static int RemoveUser { get; private set; } = 6;

        public static int Ping { get; private set; } = 666;

        public static int RsaKey { get; private set; } = 7;
        public static int AesKey { get; private set; } = 8;

        public static int KeyPass { get; private set; } = 9;
        public static int ChangeStatus { get; private set; } = 10;

        public static int ChangeName { get; private set; } = 11;

        public static int RFileSend { get; private set; } = 12;
        public static int AcceptFile { get; private set; } = 13;
        public static int CancelFile { get; private set; } = 14;
        public static int FileInfo { get; private set; } = 15;
        public static int FileData { get; private set; } = 16;
        public static int FileOK { get; private set; } = 17;

        public static int FileOKs { get; private set; } = 17;
    }
}
