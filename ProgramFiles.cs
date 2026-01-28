using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace RIKA_IMBANIKA_LIFE_HELPER
{
    public static class ProgramFiles
    {
        private static string _programFilesPathFilePath;
        private static string _dirtyDDDID;
        private static string _line;
        public static string[] _subdirs;
        public static int _goodStatus;

        public static void Init()
        {
            _subdirs = new string[]
            {
                "Audios",
                "Avas",
                "Text"
            };

            _goodStatus = _subdirs.Length;

            string current = Environment.CurrentDirectory;
            _programFilesPathFilePath = $"{current}\\PFP";

            _line = $"============================================\r\n";
            _dirtyDDDID = _line +
                          $"We need ProgramFiles folder of that app.\r\n" +
                          $"U know, that app ProgramFiles folder\r\n" +
                          $"contains that app program files.\r\n" +
                          $"Please, select normal one.\r\n" +
                          $"It should contain this app folders and files.\r\n" +
                          $"If U get this error it means that\r\n" +
                          $"something missing in folder we checking,\r\n" +
                          $"or this is absolutelly another folder.\r\n" +
                          $"Normal one should contain \"avocado\" file,\r\n" +
                          $"and firstly was named\r\n" +
                          $"\"{S.AppName} PROGRAM FILES\"\r\n";

            int status = -1;
            string avocado = "not even init";

            bool pfpfpExists = File.Exists(_programFilesPathFilePath);

            if (!pfpfpExists)
            {
                Bad();
                return;
            }

            Stream stream = File.OpenRead(_programFilesPathFilePath);
            BinaryReader br = new BinaryReader(stream);
            S.PF = $"{br.ReadString()}";
            br.Dispose();
            stream.Dispose();

            string avocadoPath = $"{S.PF}avocado";

            bool exists = Directory.Exists(S.PF);
            if (exists)
                status = CheckFolder(S.PF);
            bool avocadoExists = File.Exists(avocadoPath);
            bool avocadoIsNormal = false;
            if (avocadoExists)
                avocadoIsNormal = CheckAvocado(avocadoPath);

            avocado = "not even exists";

            if (avocadoExists)
                if (avocadoIsNormal)
                    avocado = "good";
                else
                    avocado = "CORRUPTED";

            if (!exists)
                Bad();
            else if (!avocadoExists && status == _goodStatus) //exists
                GoodButNoAvocado();
            else if (!avocadoExists && status < _goodStatus && status > 0) //exists
                Corrupted();
            else if (!avocadoExists && status == 0) //exists
                Bad();
            else if (status == _goodStatus && !avocadoIsNormal) //exists, avocadoExists
                AvocadoCorrupted();
            else if (status < _goodStatus && !avocadoIsNormal) //exists, avocadoExists
                Corrupted();
            else if (status < _goodStatus) //exists, avocadoExists, avocadoIsNormal
                Corrupted();
            else //exists, avocadoExists, avocadoIsNormal, status = _goodStatus
                Good();

            void Good()
            {
                //Good. We should to do nothing.
                return;
            }

            void GoodButNoAvocado()
            {
                MessageBox.Show(
                    $"{S.GetHello()}\r\n" +
                    $"There is no AvOcAdO teso!\r\n" +
                    $"So I will create it by myself... Nya!\r\n" +
                    $"Everything else is normal... GOOD LUCK!\r\n" +
                    _line +
                    $"INFO:\r\n" +
                    $"Saved ProgramFiles path: \r\n\"{S.PF}\",\r\n" +
                    $"Current app path: \r\n\"{Directory.GetCurrentDirectory()}\",\r\n" +
                    $"Avocado: {avocado}.",
                    $"{S.AppName} - NO AVOCADO!", MessageBoxButton.OK);

                CreateAvocado(avocadoPath);
                return;
            }
            void AvocadoCorrupted()
            {
                MessageBox.Show(
                   $"{S.GetHello()} Folder is normal, but...\r\n" +
                   $"AVOCADO IS CORRUPTED!\r\n" +
                   $"HOW SHOULD PROGRAM WORK WITH CORRUPTED AVOCADO?!\r\n" +
                   $"DID U THINK PROGRAMS CAN WORK WITHOUT AVOCADOS?\r\n" +
                   $"IMPOSSIBLE...\r\n" +
                   $"No no no no no no no no no no no no no no no\r\n" +
                   $"Select folder with uncorrupted avocado please.\r\n" +
                   $"* Or U can just convert text \"RICHARD GAY!\" in binary code.\r\n" +
                   $"And write it inside \"{S.PF}\\avocado\"" +
                   $"This should did the thing..." +
                   $"(Or just delete it and I will create new avocado just for u)" +
                  _line +
                   $"INFO:\r\n" +
                   $"Saved ProgramFiles path: \r\n\"{S.PF}\",\r\n" +
                   $"Current app path: \r\n\"{Directory.GetCurrentDirectory()}\",\r\n" +
                   $"Folder status: {status}\r\n" +
                   $"Avocado: {avocado}.",
                   $"{S.AppName} - AVOCADO IS CORRUPTED!!!", MessageBoxButton.OK);

                GetNewPath(S.PF);
            }

            void Corrupted()
            {
                MessageBox.Show($"{S.GetHello()}\r\n" +
                    $"Sorry, but ProgramFiles of that app folder was corrupted.\r\n" +
                    $"Find or download correct one.\r\n" +
                    $"Than click OKAY select it pls.\r\n" +
                    _dirtyDDDID +
                    _line +
                    $"INFO:\r\n" +
                    $"Saved ProgramFiles path: \r\n\"{S.PF}\",\r\n" +
                    $"Current app path: \r\n\"{Directory.GetCurrentDirectory()}\",\r\n" +
                    $"Folder status: {status}\r\n" +
                    $"Avocado: {avocado}.",
                    $"{S.AppName} - Program files folder is kinda corrupted.", MessageBoxButton.OK);

                GetNewPath(S.PF);
            }

            void Bad()
            {
                string res = TrySearch();

                if (!res.Equals("No."))
                {
                    MessageBox.Show($"{S.GetHello()}\r\n" +
                    $"ProgramFiles path was changed from\r\n" +
                    $"\"{S.PF}\"\r\n" +
                    $"to\r\n" +
                    $"{res}.\r\n" +
                    $"Good Luck Teso!\r\n",
                    $"{S.AppName} - Info.", MessageBoxButton.OK);

                    ChangePFP(res);
                }
                else
                {
                    MessageBox.Show($"{S.GetHello()}\r\n" +
                        $"Sorry, but I can't find ProgramFiles folder.\r\n" +
                        $"That means it doesn't even located somewhere in folder with application\r\n" +
                        $"So if U know where it is, please press OKAY, and select it.\r\n" +
                        _dirtyDDDID +
                        _line +
                        $"INFO:\r\n" +
                        $"Saved ProgramFiles path: \r\n\"{S.PF}\",\r\n" +
                        $"Current app path: \r\n\"{Directory.GetCurrentDirectory()}\",\r\n" +
                        $"Folder status: {status}\r\n" +
                        $"Avocado: {avocado}.",
                        $"{S.AppName} - I CAN'T FIND PROGRAM FILES FOLDER!", MessageBoxButton.OK);

                    GetNewPath(S.PF);
                }

                string TrySearch()
                {
                    try
                    {
                        DirectoryInfo dick = Directory.GetParent(S.PF);
                        string path = dick.FullName;

                        string a1 = Search(path);
                        if (!a1.Equals("No."))
                            return a1;
                        else
                        {
                            dick = Directory.GetParent(Environment.CurrentDirectory);
                            path = dick.FullName;
                            return Search(path);
                        }
                    }
                    catch
                    {
                        return "No.";
                    }

                    string Search(string path)
                    {
                        bool okay = Check(path);

                        if (okay)
                            return path;
                        else
                        {
                            string[] dirs = Directory.GetDirectories(path);

                            foreach (string dir in dirs)
                                if (!Search(dir).Equals("No."))
                                    return dir;

                            return "No.";
                        }
                    }



                    bool Check(string path)
                    {
                        if (!CheckAvocadoInFolder(path))
                            return false;
                        else
                            return (CheckFolder(path) == _goodStatus);
                    }
                }
            }
        }

        static void GetNewPath(string startPath)
        {
            OpenFolderDialog dialog;
            string pfp;
            bool? goodDialog;
            string avocadoPath;
            bool avocadoExists;
            int folderStatus;
            bool avocado;
            string avocadoStatus;

            while (true)
            {
                if (!Directory.Exists(startPath))
                    startPath = Environment.CurrentDirectory;

                dialog = new OpenFolderDialog();
                dialog.FolderName = startPath;
                dialog.InitialDirectory = startPath;
                dialog.Multiselect = false;

                pfp = "Not yet.";

                goodDialog = dialog.ShowDialog();

                if (goodDialog != true)
                {
                    BadDialog();
                    continue;
                }

                pfp = $"{dialog.FolderName}\\";

                if (!Directory.Exists(pfp))
                {
                    UnexistingDirectory();
                    continue;
                }

                avocadoPath = $"{pfp}avocado";
                avocadoExists = File.Exists(avocadoPath);
                folderStatus = CheckFolder(pfp);

                avocado = false;
                if (avocadoExists)
                    avocado = CheckAvocado(avocadoPath);

                avocadoStatus = avocado ? "good" : "incorrect";

                if (!avocadoExists && folderStatus <= 0) //exists
                    Incorrect();
                else if (!avocadoExists && folderStatus == _goodStatus) //exists
                {
                    GoodButNoAvocado();
                    break;
                }
                else if (!avocadoExists) //exists
                    Corrupt();
                else if (!avocado && folderStatus == _goodStatus)
                    CorruptedAvocado();
                else if (!avocado && folderStatus == 0) //exists, avocadoExists
                    Incorrect();
                else if (!avocado) //exists, avocadoExists, status 1-4
                    Corrupt();
                else if (avocado && folderStatus < _goodStatus) //exists, avocadoExists, avocado
                    Corrupt();
                else //exists, avocadoExists, avocado, folderStatus = _goodStatus
                {
                    Final();
                    break;
                }
            }

            void GoodButNoAvocado()
            {
                MessageBox.Show($"{S.GetHello()}\r\n" +
                        $"THERE IS NO AVOCADO TESO!\r\n" +
                        $"So I will create new one by myself... Nya!\r\n" +
                        $"Everything else is normal... GoodLuck!\r\n" +
                        _line +
                        $"INFO:\r\n" +
                        $"Selected folder (normal): {pfp}\r\n" +
                        $"Avocado status: not even exists \r\n" +
                        $"Folder status: {folderStatus}\r\n" +
                        $"Dialog: {dialog}\r\n" +
                        $"Dialog result: {goodDialog}\r\n" +
                        $"Saved ProgramFiles path: {S.PF}.\r\n" +
                        $"Current app path: {Directory.GetCurrentDirectory()}.",
                        $"{S.AppName} - NO AVOCADO!", MessageBoxButton.OK);

                CreateAvocado(avocadoPath);
                Final();
            }

            void UnexistingDirectory()
            {
                MessageBox.Show($"{S.GetHello()}\r\n" +
                            $"LOL! How did U select unexisting folder?\r\n" +
                            $"This is possible, but why or how did U do that? Nya!\r\n" +
                            $"So LETS DO THIS AGAIN. NYA!\r\n" +
                            _dirtyDDDID +
                            _line +
                            $"INFO:\r\n" +
                            $"Selected folder (kinda not exits): {pfp}\r\n" +
                            $"Dialog: {dialog}\r\n" +
                            $"Dialog result: {goodDialog}\r\n" +
                            $"Saved ProgramFiles path: {S.PF}.\r\n" +
                            $"Current app path: {Directory.GetCurrentDirectory()}.",
                            $"{S.AppName} - UNEXISTING FOLDER.", MessageBoxButton.OK);
            }

            void Incorrect()
            {
                MessageBox.Show(
                       $"{S.GetHello()}\r\n" +
                       $"This is incorrect folder teso!\r\n" +
                       _dirtyDDDID +
                       _line +
                       $"INFO:\r\n" +
                       $"Avocado status: {avocadoStatus} \r\n" +
                       $"Folder status: 0\r\n" +
                       $"Selected folder (incorrect): {pfp}\r\n" +
                       $"Dialog: {dialog}\r\n" +
                       $"Dialog result: {goodDialog}\r\n" +
                       $"Saved ProgramFiles path: {S.PF}.\r\n" +
                       $"Current app path: {Directory.GetCurrentDirectory()}.",
                       $"{S.AppName} - Incorrect folder.", MessageBoxButton.OK);
            }

            void CorruptedAvocado()
            {
                MessageBox.Show(
                           $"{S.GetHello()}\r\n" +
                           $"Folder is good, but..." +
                           $"AVOCADO IS CORRUPTED!\r\n" +
                           $"HOW SHOULD PROGRAM WORK WITH CORRUPTED AVOCADO!\r\n" +
                           $"DID U THINK PROGRAMS CAN WORK WITHOUT AVOCADOS?\r\n" +
                           $"IMPOSSIBLE...\r\n" +
                           $"No no no no no no no no no no no no no no no\r\n" +
                           $"0_0\r\n" +
                           $"* Or U can just convert text \"RICHARD GAY!\" in binary code.\r\n" +
                           $"And write it inside \"{S.PF}\\avocado\"" +
                           $"This should did the thing..." +
                           $"(Or just delete it and I will create new avocado just for u)" +
                           _line +
                           $"INFO:\r\n" +
                           $"Selected folder (corrupt): {pfp}\r\n" +
                           $"Avocado status: {avocadoStatus} \r\n" +
                           $"Folder status: {folderStatus}\r\n" +
                           $"Dialog: {dialog}\r\n" +
                           $"Dialog result: {goodDialog}\r\n" +
                           $"Saved ProgramFiles path: {S.PF}.\r\n" +
                           $"Current app path: {Directory.GetCurrentDirectory()}.",
                           $"{S.AppName} - AVOCADO IS CORRUPTED!!!", MessageBoxButton.OK);
            }

            void Corrupt()
            {
                MessageBox.Show(
                     $"{S.GetHello()}\r\n" +
                     $"HOLY CRINGE, this folder is corrupted.\r\n" +
                     $"Please, find select normal one.\r\n" +
                     $"So LETS DO THIS AGAIN! NYA!\r\n" +
                     _dirtyDDDID +
                     _line +
                     $"INFO:\r\n" +
                     $"Selected folder (corrupt): {pfp}\r\n" +
                     $"Avocado status: {avocadoStatus} \r\n" +
                     $"Folder status: {folderStatus}\r\n" +
                     $"Dialog: {dialog}\r\n" +
                     $"Dialog result: {goodDialog}\r\n" +
                     $"Saved ProgramFiles path: {S.PF}.\r\n" +
                     $"Current app path: {Directory.GetCurrentDirectory()}",
                     $"{S.AppName} - Corrupted folder.", MessageBoxButton.OK);
            }

            void BadDialog()
            {
                MessageBox.Show(
                    $"{S.GetHello()}\r\n" +
                    $"Sorry, something wrong with OpenFolderDialog.\r\n" +
                    $"So LETS DO THIS AGAIN! NYA!\r\n" +
                    _dirtyDDDID +
                    _line +
                    $"INFO:\r\n" +
                    $"Dialog: {dialog}\r\n" +
                    $"Dialog result: {goodDialog}\r\n" +
                    $"Dialog selected folder: {dialog.FolderName}\r\n" +
                    $"Saved ProgramFiles path: {S.PF}.\r\n" +
                    $"Current app path: {Directory.GetCurrentDirectory()}.",
                    $"{S.AppName} - OpenFolderDialog not worked...", MessageBoxButton.OK);
            }

            void Final()
            {
                ChangePFP(pfp);
            }
        }

        public static bool CheckAvocadoInFolder(string path)
        {
            return CheckAvocado($"{path}\\avocado");
        }

        public static bool CheckAvocado(string avocadoPath)
        {
            Stream stream = File.OpenRead(avocadoPath);
            BinaryReader br = new BinaryReader(stream);
            string text = br.ReadString();
            br.Dispose();
            stream.Dispose();

            return text.Equals($"RICHARD GAY!");
        }

        public static void CreateAvocado(string avocadoPath)
        {
            Stream stream = File.OpenWrite(avocadoPath);
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write($"RICHARD GAY!");
            bw.Dispose();
            stream.Dispose();
        }

        public static int CheckFolder(string path)
        {
            int wtf = _subdirs.Length;

            string[] subdirs = Directory.GetDirectories(path);

            if (subdirs.Length <= 0)
                return 0;

            for (int i = 0; i < _subdirs.Length; i++)
            {
                if (!subdirs.Contains($"{path}{_subdirs[i]}"))
                    wtf--;
            }

            return wtf;
        }

        public static void ChangePFP(string res)
        {
            Stream stream = File.OpenWrite(_programFilesPathFilePath);
            BinaryWriter br = new BinaryWriter(stream);
            br.Write(res);
            br.Dispose();
            stream.Dispose();

            S.PF = $"{res}\\";
        }
    }
}
