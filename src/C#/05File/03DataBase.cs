//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Scripting;
//using Microsoft.CodeAnalysis.Emit;
//using Microsoft.CodeAnalysis.Scripting;
//using Microsoft.Data.Sqlite;
//using Newtonsoft.Json;//Json
//using System;//Action
//using System.Collections;
//using System.Collections.Generic;//List
//using System.Diagnostics;
//using System.IO;//File
//using System.Linq;//from XX select XX
//using System.Reflection;
//using System.Runtime.InteropServices;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Timers;//Timer
//using TMPro;//InputField
//using UnityEngine;//Mono
//using UnityEngine.Tilemaps;
//using UnityEngine.UI;//Image
//using UnityEngine.Video;//Vedio
//using static CMKZ.LocalStorage;
//using static UnityEngine.Object;//Destory
//using static UnityEngine.RectTransform;

//namespace CMKZ {
//    public static partial class LocalStorage {
//        public static bool IsSQLiteInit;
//        public static void InitSQLite() {
//            if (!IsSQLiteInit) {
//                IsSQLiteInit = true;
//                SQLitePCL.Batteries.Init();
//            }
//        }
//        public static void SQLiteExe(string 数据库名, string 指令) {
//            InitSQLite();
//            using var A = new SqliteConnection($"Data Source={CMKZDir}{数据库名}.db;Pooling=false");
//            A.Open();
//            using var B = A.CreateCommand();
//            B.CommandText = 指令;
//            B.ExecuteNonQuery();
//        }
//    }
//    public class SqliteExampleWithMicrosoftData : MonoBehaviour {
//        void Start() {
//            // 数据库文件的路径，这里放在项目的Assets文件夹下
//            string dbPath = "Data Source=" + Application.dataPath + "/myDatabase.db";
//            QueryData(dbPath);


//            SQLiteExe("Test", "CREATE TABLE IF NOT EXISTS Scores (name TEXT, score INTEGER)");
//            SQLiteExe("Test", "INSERT INTO Scores (name, score) VALUES ('Player', 100)");
//        }
//        private void QueryData(string dbPath) {
//            using var connection = new SqliteConnection(dbPath);
//            connection.Open();
//            using var command = connection.CreateCommand();
//            command.CommandText = "SELECT name, score FROM Scores";
//            using var reader = command.ExecuteReader();
//            while (reader.Read()) {
//                Print("Name: " + reader.GetString(0) + ", Score: " + reader.GetInt32(1));
//            }
//        }
//    }
//    public class 数据库测试 {
//        public void 创建测试() {
//            SQLiteExe("Test", "CREATE TABLE IF NOT EXISTS Scores (name TEXT, score INTEGER)");
//            Print(FileExists("Test"));
//        }
//    }
//}