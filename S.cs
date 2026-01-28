using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_IMBANIKA_LIFE_HELPER
{
    public static class S
    {
        private static string _appName;
        private static string _programFiles;
        private static string[] _hello;
        private static int _helloId;

        static S()
        {
            _appName = "RIKA IMBANIKA LIFE HELPER";

            _hello = new string[]
            {
                "HELLO!",
                "HELLO!!!",
                "SALUT!",
                "CONNICHUA!",
                "CONNICHUA!!!",
                "CONNICHUA!!!!!",
                "OH MY!",
                "GREETINGS!",
                "WHAT?",
                "OMG OMG OMG",
                "HOLY COW!",
                "SO U ARE A MUSICIAN?",
                "SO...",
                "HMMMMMMM",
                "DO U LIKE PONIES? PONIES ARE CUTE!",
                "CONGRATULATIONS!",
                "GOOD, BUT...",
                "INTRESTING!",
                "IMPOSSIBLE...",
                "HOW DOES THAT HAPPENS?",
                "HOW ARE U?",
                "HELLO, HOW ARE U?",
                "WELCOME!",
                "WELCOME BACK!",
                "I LOVE U! BUT...",
                "HI!",
                "U GOOD!",
                "HELLO TESO!",
                "HELLO TESO!!!",
                "NYA!",
                "NYYYAAA!",
                "AAAAAAAAAAAAAAAHHHHHHHHHHH"
            };

            ShuffleHello();

            ProgramFiles.Init();
        }

        public static void Init()
        {
            _helloId = _helloId;
        }

        public static string GetHello()
        {
            _helloId++;

            if (_helloId >= _hello.Length)
                ShuffleHello();

            return _hello[_helloId];
        }

        public static void ShuffleHello()
        {
            Random rnd = new Random();
            string[] cringe = new string[_hello.Length];
            string last = _hello[_hello.Length - 1];

            for (int i = 0; i < _hello.Length;)
            {
                int kek = rnd.Next(cringe.Length);
                if (string.IsNullOrEmpty(cringe[kek]))
                {
                    cringe[kek] = _hello[i];

                    if (i == _hello.Length - 1 && kek == 0)
                    {
                        cringe[kek] = cringe[i];
                        cringe[i] = _hello[i];
                        break;
                    }

                    i++;
                }
            }

            _hello = cringe;
            _helloId = 0;
        }

        public static string AppName
        {
            get
            {
                return _appName;
            }
            set
            {
                _appName = value;
            }
        }

        public static string PF
        {
            get
            {
                return _programFiles;
            }
            set
            {
                _programFiles = value;
            }
        }
    }
}
