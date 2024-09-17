using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CMKZ {
    public class 主键Attribute : Attribute { }
    public static partial class LocalStorage {
        public static CMKZ_Dictionary<Type, string> SQL类型 = new() {
            [typeof(sbyte)] = "tinyint",
            [typeof(byte)] = "tinyint unsigned",
            [typeof(short)] = "smallint",
            [typeof(ushort)] = "smallint unsigned",
            [typeof(int)] = "int",
            [typeof(uint)] = "int unsigned",
            [typeof(long)] = "bigint",
            [typeof(ulong)] = "bigint unsigned",
            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(decimal)] = "decimal",
            [typeof(bool)] = "int",
            [typeof(char)] = "char(1)",
            [typeof(string)] = "varchar(255)",  // 默认长度为255，你可以根据需要调整
            [typeof(DateTime)] = "datetime",
            [typeof(TimeSpan)] = "time",
            [typeof(byte[])] = "blob",
            [typeof(Guid)] = "char(36)",  // UUID 通常存储为36字符的字符串
            [typeof(DateOnly)] = "date",
            [typeof(TimeOnly)] = "time"
        };
        public static string SQLName = "CMKZ";
        public static string SQLPassWord;
        public static void SetSQLName(string X) {
            SQLName = X;
        }
        public static void SetSQLPassword(string X) {
            SQLPassWord = X;
            数据库服务连接文本 = new MySqlConnectionStringBuilder {
                Server = "localhost",
                UserID = "root",
                Password = X,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 100,
                ConnectionTimeout = 30
            }.ToString();
        }
        public static string 数据库服务连接文本;
        public static string _数据库连接文本;
        public static string 数据库连接文本 {
            get {
                if (_数据库连接文本 == null) {
                    _数据库连接文本 = new MySqlConnectionStringBuilder {
                        Server = "localhost",
                        UserID = "root",
                        Password = SQLPassWord,
                        Database = SQLName,
                        Pooling = true,
                        MinimumPoolSize = 0,
                        MaximumPoolSize = 100,
                        ConnectionTimeout = 30
                    }.ToString();
                }
                return _数据库连接文本;
            }
        }
        public static HashSet<string> 数据库创建缓存 = new();
        public static HashSet<string> 数据表创建缓存 = new();
        public static void SQLCreateDataBase(string X) {
            if (数据库创建缓存.Contains(X)) {
                return;
            }
            using var connection = new MySqlConnection(数据库服务连接文本);
            connection.Open();
            string checkDbQuery = $"SHOW DATABASES LIKE '{X}';";
            using var checkDbCmd = new MySqlCommand(checkDbQuery, connection);
            var result = checkDbCmd.ExecuteScalar();
            if (result == null) {
                using var createDbCmd = new MySqlCommand($"CREATE DATABASE {X};", connection);
                createDbCmd.ExecuteNonQuery();
                Print($"数据库 '{X}' 创建成功。");
            }
            数据库创建缓存.Add(X);
        }
        public static void SQLCreateTable<T>() {
            if (数据表创建缓存.Contains(typeof(T).Name)) {
                return;
            }
            SQLCreateDataBase(SQLName);
            using var connection = new MySqlConnection(数据库连接文本);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try {
                var columns = typeof(T).GetFields();
                var A = new StringBuilder();
                for (int i = 0; i < columns.Length; i++) {
                    if (columns[i].GetCustomAttribute<主键Attribute>() != null) {
                        A.Append(columns[i].Name).Append(" ").Append(SQL类型[columns[i].FieldType]).Append(" PRIMARY KEY");
                    } else {
                        A.Append(columns[i].Name).Append(" ").Append(SQL类型[columns[i].FieldType]);
                    }
                    if (i < columns.Length - 1) {
                        A.Append(",");
                    }
                }
                Print($"CREATE TABLE IF NOT EXISTS {typeof(T).Name} ({A});");
                using (var cmd = new MySqlCommand($"CREATE TABLE IF NOT EXISTS {typeof(T).Name} ({A});", connection, transaction)) {
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            } catch (Exception ex) {
                transaction.Rollback();
                PrintWarning($"SQL ERROR：建表错误 {ex.Message}");
            }
            数据表创建缓存.Add(typeof(T).Name);
        }
        public static void SQLUpdate<T>(string ID,string 昵称,string 列,string 值) {
            SQLCreateTable<T>();
            //var query = $"UPDATE {typeof(T).Name} SET {列} = @value WHERE ID = @id;";
            //var query = $@"INSERT INTO {typeof(T).Name} (ID, {列}) VALUES (@id, @value) ON DUPLICATE KEY UPDATE {列} = @value;";
            var query = $@"
    INSERT INTO {typeof(T).Name} (SteamID, Steam昵称, {列})
    VALUES (@id, @nickname, @value)
    ON DUPLICATE KEY UPDATE
    {列} = CASE
        WHEN {列} = '00:00:00' OR {列} IS NULL OR {列} > @value THEN @value
        ELSE {列}
    END;";

            using var connection = new MySqlConnection(数据库连接文本);
            connection.Open();
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@value", 值);
            cmd.Parameters.AddWithValue("@nickname", 昵称);
            cmd.Parameters.AddWithValue("@id", ID);
            cmd.ExecuteNonQuery();
        }
        public static void SQLSave<T>(T X) {
            SQLCreateTable<T>();
            //using var connection = new MySqlConnection(数据库连接文本);
            //connection.Open();
            //using var transaction = connection.BeginTransaction();
            //try {
            //    var columns = typeof(T).GetFields();
            //    var A = new List<string>();
            //    var B = new List<string>();
            //    var C = new List<string>();
            //    for (int i = 0; i < columns.Length; i++) {
            //        A.Add(columns[i].Name);
            //        B.Add($"'{columns[i].GetValue(X)}'");
            //        if (columns[i].GetCustomAttribute<主键Attribute>() == null) {
            //            C.Add($"{columns[i].Name}='{columns[i].GetValue(X)}'");
            //        }
            //    }
            //    var 命令 = $"INSERT INTO {typeof(T).Name} ({A.Join(",")}) VALUES ({B.Join(",")}) ON DUPLICATE KEY UPDATE {C.Join(",")}";
            //    using (var cmd = new MySqlCommand(命令, connection, transaction)) {
            //        cmd.ExecuteNonQuery();
            //    }
            //    transaction.Commit();
            //} catch (Exception ex) {
            //    transaction.Rollback();
            //    PrintWarning($"SQL ERROR：存档错误 {ex.Message}");
            //}
            _queues.GetOrAdd(typeof(T), new ConcurrentQueue<object>()).Enqueue(X);
        }
        public static T SQLLoad<T>(object ID) where T:new() {
            SQLCreateTable<T>();
            using var connection = new MySqlConnection(数据库连接文本);
            connection.Open();
            using var cmd = new MySqlCommand($"SELECT * FROM {typeof(T).Name} WHERE {typeof(T).GetFields().First(t => t.GetCustomAttribute<主键Attribute>() != null).Name}='{ID}'", connection);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) {
                var X = Activator.CreateInstance<T>();
                var columns = typeof(T).GetFields();
                for (int i = 0; i < columns.Length; i++) {
                    //如果columns[i]是bool类型，而数据库中存储的是int类型，需要进行转换
                    if (columns[i].FieldType == typeof(bool)) {
                        columns[i].SetValue(X, Convert.ToBoolean(reader[columns[i].Name]));
                    } else {
                        //检测null值避免setValue报错
                        if (reader[columns[i].Name] == DBNull.Value) {
                            continue;
                        }
                        columns[i].SetValue(X, reader[columns[i].Name]);
                    }
                }
                return X;
            }
            PrintWarning($"SQL WARNING：SQLLoad {ID} 不存在。返回默认");
            return new T();
        }
        public static CMKZList<T> SQLForall<T>(string 列名, bool 最大, int 数量) {
            SQLCreateTable<T>();
            using var connection = new MySqlConnection(数据库连接文本);
            connection.Open();
            //using var cmd = new MySqlCommand($"SELECT * FROM {typeof(T).Name} ORDER BY {列名} {(最大 ? "DESC" : "ASC")} LIMIT {数量}", connection);
            using var cmd = new MySqlCommand($"SELECT * FROM {typeof(T).Name} WHERE TIME_TO_SEC({列名}) > 1 ORDER BY {列名} {(最大 ? "DESC" : "ASC")} LIMIT {数量}", connection);
            
            using var reader = cmd.ExecuteReader();
            var A = new CMKZList<T>();
            while (reader.Read()) {
                var X = Activator.CreateInstance<T>();
                var columns = typeof(T).GetFields();
                for (int i = 0; i < columns.Length; i++) {
                    if (reader[columns[i].Name] == DBNull.Value) {
                        continue;
                    }
                    columns[i].SetValue(X, reader[columns[i].Name]);
                }
                A.Add(X);
            }
            return A;
        }
        public static int SQLRank<T>(string 列名, bool 最大, object ID) {
            SQLCreateTable<T>();
            using var connection = new MySqlConnection(数据库连接文本);
            connection.Open();
            using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {typeof(T).Name} WHERE TIME_TO_SEC({列名}) > 1 AND {列名} {(最大 ? ">" : "<")} (SELECT {列名} FROM {typeof(T).Name} WHERE {typeof(T).GetFields().First(t => t.GetCustomAttribute<主键Attribute>() != null).Name}='{ID}')", connection);
            //Print($"SELECT COUNT(*) FROM {typeof(T).Name} WHERE {列名} {(最大 ? ">" : "<")} (SELECT {列名} FROM {typeof(T).Name} WHERE {typeof(T).GetFields().First(t => t.GetCustomAttribute<主键Attribute>() != null).Name}='{ID}')");
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _queues = new();
        private static readonly Timer _timer = new(ProcessQueues, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        public static void SQLWriteAll() => ProcessQueues(null);
        private static void ProcessQueues(object state) {
            foreach (var i in _queues) {
                var queue = i.Value;
                if (queue.IsEmpty)
                    continue;
                var items = new CMKZList<object>();
                while (queue.TryDequeue(out var item)) {
                    items.Add(item);
                }
                if (items.Count > 0) {
                    BulkInsert(i.Key, items);
                }
            }
        }
        public static CMKZList<object> Replace(this CMKZList<object> X, object Y, object Z) {
            for (int i = 0; i < X.Count; i++) {
                if (X[i].Equals(Y)) {
                    X[i] = Z;
                }
            }
            return X;
        }
        private static void BulkInsert(Type type, CMKZList<object> items) {
            items = items.Replace(true, 1).Replace(false, 0);
            using var connection = new MySqlConnection(数据库连接文本);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            //try {
            var columns = type.GetFields();
            var tableName = type.Name;
            var insertCommand = $"INSERT INTO {tableName} ({string.Join(",", columns.Select(c => c.Name))}) VALUES ";
            var valueRows = new CMKZList<string>();
            var updateCommand = " ON DUPLICATE KEY UPDATE ";
            var updateColumns = columns.Where(c => c.GetCustomAttribute<主键Attribute>() == null)
                                        .Select(c => $"{c.Name}=VALUES({c.Name})");
            foreach (var item in items) {
                var values = columns.Select(c => {
                    var value = c.GetValue(item);
                    if (value is bool boolValue) // 检查值是否为布尔型
                        return boolValue ? "1" : "0"; // 将布尔值转换为 "1" 或 "0"
                    else
                        return $"'{value}'"; // 其他类型的值直接转换为字符串
                });
                valueRows.Add($"({string.Join(",", values)})");
            }
            insertCommand += string.Join(",", valueRows);
            insertCommand += updateCommand + string.Join(",", updateColumns);
            using (var cmd = new MySqlCommand(insertCommand, connection, transaction)) {
                cmd.ExecuteNonQuery();
            }
            transaction.Commit();
            //} catch (Exception ex) {
            //    transaction.Rollback();
            //    PrintWarning($"SQL ERROR: {ex.Message}");
            //}
        }
    }
}