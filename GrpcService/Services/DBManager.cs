using GrpcService.Classes;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Service;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace GrpcService.Services
{
    public class DBManager : DBService.DBServiceBase
    {
        ApplicationContext db = new ApplicationContext();

        public override Task<CreateDBReply> CreateDB(CreateDBRequest request, ServerCallContext context)
        {
            var resultReply = new CreateDBReply();
            if (request.Name.Equals(""))
                resultReply.Id = 0;
            else
            {
                Database database = new Database(request.Name);
                database.DBPath = "";
                db.Databases.Add(database);
                db.SaveChanges();
                resultReply.Id = database.Id;
            }
            return Task.FromResult(resultReply);
        }

        public override Task<DBsReply> GetDBs(Empty request, ServerCallContext context)
        {
            var DBsReply = new DBsReply();
            var dbs = db.Databases.ToList();
            foreach (var db in dbs)
            {
                DBsReply.DBs.Add(new DBReply { Id = db.Id, Name = db.DBName });
            }
            return Task.FromResult(DBsReply);
        }

        public override Task<ResultReply> AddTable(CreateTableRequest request, ServerCallContext context)
        {
            var resultReply = new ResultReply();
            if (request.Name.Equals(""))
                resultReply.Result = false;
            else if (db.Tables.Where(x => x.DatabaseId == request.DbId && x.TableName == request.Name).Count() > 0)
                resultReply.Result = false;
            else
            {
                Table table = new Table();
                table.TableName = request.Name;
                table.DatabaseId = request.DbId;
                db.Tables.Add(table);
                db.SaveChanges();

                resultReply.Result = true;
            }
            return Task.FromResult(resultReply);
        }

        public override Task<TablesReply> GetTables(GetTablesRequest request, ServerCallContext context)
        {
            var tablesReply = new TablesReply();
            var tables = db.Tables.Where(x => x.DatabaseId == request.DbId).ToList();
            foreach (var table in tables)
            {
                tablesReply.Tables.Add(new TableReply { Id = table.Id, Name = table.TableName });
            }
            return Task.FromResult(tablesReply);
        }

        public override Task<TableReply> GetTable(GetTableRequest request, ServerCallContext context)
        {
            var tableReply = new TableReply();

            var table = db.Tables.Where(x => x.Id == request.TableId).Include(x => x.Columns).Include(x => x.Rows).FirstOrDefault();
            if (table == null)
                tableReply = null;
            else
            {
                tableReply.Id = table.Id;
                tableReply.Name = table.TableName;
                if (table.Columns != null)
                {
                    foreach (var col in table.Columns)
                    {
                        ColumnReply columnReply = new ColumnReply();
                        columnReply.ColumnId = col.Id;
                        columnReply.Name = col.ColName;
                        columnReply.Type = db.Types.Find(col.TypeId).Name;
                        tableReply.Columns.Add(columnReply);
                    }
                    int index = 0;
                    if (table.Rows != null)
                    {
                        foreach (var row in table.Rows)
                        {
                            RowReply rowReply = new RowReply();
                            rowReply.Index = index;
                            var values = JsonConvert.DeserializeObject<List<string>>(row.RowValues);
                            foreach (var val in values)
                            {
                                RowValue rowValue = new RowValue();
                                rowValue.Value = val;
                                rowReply.Values.Add(rowValue);
                            }
                            index++;
                            tableReply.Rows.Add(rowReply);
                        }
                    }
                }
            }
            return Task.FromResult(tableReply);
        }

        public override Task<TypesReply> GetTypes(Empty request, ServerCallContext context)
        {
            var typesReply = new TypesReply();
            var types = db.Types.ToList();
            foreach (var type in types)
            {
                typesReply.Types_.Add(new TypeReply { Id = type.Id, Name = type.Name });
            }
            return Task.FromResult(typesReply);
        }

        public override Task<ResultReply> AddColumn(AddColumnRequest request, ServerCallContext context)
        {
            var resultReply = new ResultReply();
            var table = db.Tables.Where(x => x.Id == request.TableId).Include(x => x.Columns).Include(x => x.Rows).FirstOrDefault();
            if (table == null)
                resultReply.Result = false;
            else if (db.Columns.Where(x => x.TableId == request.TableId && x.ColName == request.Name).Count() > 0)
                resultReply.Result = false;
            else
            {
                Column column = new Column();
                column.ColName = request.Name;
                column.TableId = request.TableId;
                column.TypeId = request.TypeId;
                db.Columns.Add(column);
                db.SaveChanges();

                if (table.Rows != null && table.Rows.Count > 0)
                {
                    foreach (var row in table.Rows)
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>(row.RowValues);
                        values.Add("");
                        row.RowValues = JsonConvert.SerializeObject(values);
                        db.Entry(row).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                resultReply.Result = true;
            }
            return Task.FromResult(resultReply);
        }

        public override Task<ResultReply> AddRow(AddRowRequest request, ServerCallContext context)
        {
            var resultReply = new ResultReply();
            var table = db.Tables.Where(x => x.Id == request.TableId).Include(x => x.Columns).Include(x => x.Rows).FirstOrDefault();
            if (table == null)
                resultReply.Result = false;
            else
            {
                Row row = new Row();
                row.TableId = request.TableId;
                var colsCount = table.Columns.Count;
                row.RowValues = JsonConvert.SerializeObject(new List<string>(colsCount));
                db.Rows.Add(row);
                db.SaveChanges();

                resultReply.Result = true;
            }
            return Task.FromResult(resultReply);
        }

        public override Task<ResultReply> ChangeValue(ChangeValueRequest request, ServerCallContext context)
        {
            var resultReply = new ResultReply();
            var table = db.Tables.Where(x => x.Id == request.TableId).Include(x => x.Columns).Include(x => x.Rows).FirstOrDefault();
            if (table == null)
                resultReply.Result = false;
            else
            {
                var column = db.Columns.Where(x => x.Id == request.ColumnId && x.TableId == request.TableId).Include(x => x.ColType).FirstOrDefault();
                if (column == null || !column.ColType.Validate(request.NewValue))
                    resultReply.Result = false;
                else
                {
                    var rows = db.Rows.Where(x => x.TableId == request.TableId).OrderBy(x => x.Id).ToList();
                    var rowToChange = rows[request.RIndex];
                    if (rowToChange == null)
                        resultReply.Result = false;
                    else
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>(rowToChange.RowValues);
                        var columns = db.Columns.Where(x => x.TableId == request.TableId).ToList();
                        var colIndex = columns.IndexOf(column);
                        if (values.Count == 0 || colIndex >= values.Count)
                        {
                            while (values.Count <= colIndex)
                                values.Add("");
                        }
                        values[colIndex] = request.NewValue;
                        rowToChange.RowValues = JsonConvert.SerializeObject(values);
                        db.Entry(rowToChange).State = EntityState.Modified;
                        db.SaveChanges();
                        resultReply.Result = true;
                    }
                }
            }
            return Task.FromResult(resultReply);
        }

        /*public override Task<ResultReply> DeleteColumn(DeleteColumnRequest request, ServerCallContext context)
        {
            var resultReply = new ResultReply();
            var column = db.Columns.Find(request.ColumnId);
            if (column == null)
                resultReply.Result = false;
            else
            {

            }
            return Task.FromResult(resultReply);
        }

        public override Task<ResultReply> DeleteRow(int tIndex, int rIndex)
        {
            var resultReply = new ResultReply();

            db.Tables[tIndex].Rows.RemoveAt(rIndex);
            return true;
            return Task.FromResult(resultReply);

        }*/

        public override Task<ResultReply> DeleteTable(DeleteTableRequest request, ServerCallContext context)
        {
            var resultReply = new ResultReply();
            var table = db.Tables.Where(x => x.Id == request.TableId).Include(x => x.Columns).Include(x => x.Rows).FirstOrDefault();
            if (table == null)
                resultReply.Result = false;
            else
            {
                var rowsToRemove = db.Rows.Where(x => x.TableId == request.TableId);
                foreach (var rowToRemove in rowsToRemove)
                    db.Rows.Remove(rowToRemove);
                var colsToRemove = db.Columns.Where(x => x.TableId == request.TableId);
                foreach (var colToRemove in colsToRemove)
                    db.Columns.Remove(colToRemove);
                db.Tables.Remove(table); 
                resultReply.Result = true;
            }
            return Task.FromResult(resultReply);
        }

        char sep = '$';
        char space = '#';
        public override Task<ResultReply> SaveDB(SaveDBRequest request, ServerCallContext context)
        {
            var resultReply = new ResultReply();
            var database = db.Databases.Include(x => x.Tables).ThenInclude(x => x.Columns)
                .Include(x => x.Tables).ThenInclude(t => t.Rows).FirstOrDefault(x => x.Id == request.Id);
            if (database == null)
                resultReply.Result= false;
            else
            {
                var path = @"C:\\Users\\dulko\\source\\repos\\GrpcService\\databases\\" + database.DBName + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".tdb";
                database.DBPath = path;
                db.SaveChanges();

                StreamWriter sw = new StreamWriter(path);
                sw.WriteLine(database.DBName);
                foreach (Table t in database.Tables)
                {
                    sw.WriteLine(sep);
                    sw.WriteLine(t.TableName);
                    foreach (Column c in t.Columns)
                    {
                        sw.Write(c.ColName + space);
                    }
                    sw.WriteLine();
                    foreach (Column c in t.Columns)
                    {
                        var type = db.Types.Find(c.TypeId);
                        sw.Write(type.Name + space);
                    }
                    sw.WriteLine();
                    foreach (Row r in t.Rows)
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>(r.RowValues);
                        foreach (string s in values)
                        {
                            sw.Write(s + space);
                        }
                        sw.WriteLine();
                    }
                }
                sw.Close();
                resultReply.Result = true;
            }
            return Task.FromResult(resultReply);
        }

        public override Task<DBReply> OpenDB(OpenDBRequest request, ServerCallContext context)
        {
            DBReply dBReply = new DBReply();
            StreamReader sr = new StreamReader(request.Path);
            var DB = db.Databases.Where(x => x.DBPath == request.Path).FirstOrDefault();
            if (DB != null)
                dBReply.Id = DB.Id;
            else
                dBReply.Id = 0;

            string file = sr.ReadToEnd();
            string[] parts = file.Split(sep);

            Database database = new Database(parts[0]);
            dBReply.Name = database.DBName.Replace("\r\n\r\n\r\n", "");
            for (int i = 1; i < parts.Length; ++i)
            {
                parts[i] = parts[i].Replace("\r\n", "\r");
                List<string> buf = parts[i].Split('\r').ToList();
                buf.RemoveAt(0);
                buf.RemoveAt(buf.Count - 1);

                if (buf.Count > 0)
                {
                    database.Tables.Add(new Table(buf[0]));
                }
                if (buf.Count > 2)
                {
                    string[] cname = buf[1].Split(space);
                    string[] ctype = buf[2].Split(space);
                    int length = cname.Length - 1;
                    for (int j = 0; j < length; ++j)
                    {
                        database.Tables[i - 1].Columns.Add(new Column(cname[j], ctype[j]));
                    }

                    for (int j = 3; j < buf.Count; ++j)
                    {
                        database.Tables[i - 1].Rows.Add(new Row());
                        List<string> values = buf[j].Split(space).ToList();
                        values.RemoveAt(values.Count - 1);
                        if(database.Tables[i - 1].Rows.Last().RowValues != null)
                        {
                            var rowValues = JsonConvert.DeserializeObject<List<string>>(database.Tables[i - 1].Rows.Last().RowValues);
                            rowValues.AddRange(values);
                            database.Tables[i - 1].Rows.Last().RowValues = JsonConvert.SerializeObject(rowValues);
                        }
                    }
                }
            }
            sr.Close();
            dBReply.TablesCount = database.Tables.Count();
            return Task.FromResult(dBReply);
        }

        public override Task<TablesFieldReply> GetTablesField(GetTablesFieldRequest request, ServerCallContext context)
        {
            TablesFieldReply tablesFieldReply = new TablesFieldReply();
            var table1 = db.Tables.Where(x => x.Id == request.Id1).Include(x => x.Columns).FirstOrDefault();
            var table2 = db.Tables.Where(x => x.Id == request.Id2).Include(x => x.Columns).FirstOrDefault();
            List<string> colNames = new List<string>();
            if (table1 != null && table2 != null)
            {
                if (table1.Columns.Count > 0 && table2.Columns.Count > 0)
                    colNames = table1.Columns.Select(x => x.ColName).Intersect(table2.Columns.Select(x => x.ColName)).ToList();
            }
            foreach(var col in colNames)
                tablesFieldReply.Fields.Add(new FieldReply { Field = col });
            return Task.FromResult(tablesFieldReply);
        }

        public override Task<TableReply> UnionOfTables(UnionOfTablesRequest request, ServerCallContext context)
        {
            TableReply tableReply = new TableReply();
            var table1 = db.Tables.Where(x => x.Id == request.Id1).Include(x => x.Columns).Include(x => x.Rows).FirstOrDefault();
            var table2 = db.Tables.Where(x => x.Id == request.Id2).Include(x => x.Columns).Include(x => x.Rows).FirstOrDefault();
            Table table = new Table("Union_" + request.Field);
            table.DatabaseId = table1.DatabaseId;
            db.Tables.Add(table);
            db.SaveChanges();
            
            if (table1 != null && table2 != null)
            {
                if (table1.Columns.Select(x => x.ColName).Contains(request.Field) && table2.Columns.Select(x => x.ColName).Contains(request.Field))
                {
                    foreach (var col in table1.Columns.Union(table2.Columns).ToList())
                    {
                        Column column = new Column();
                        column.ColName = col.ColName;
                        column.TableId = table.Id;
                        column.TypeId = col.TypeId;
                        column.ColType = col.ColType;
                        db.Columns.Add(column);
                    }
                    db.SaveChanges();
                    
                    var col1 = table1.Columns.Where(x => x.ColName == request.Field).FirstOrDefault();
                    var col2 = table2.Columns.Where(x => x.ColName == request.Field).FirstOrDefault();
                    if (col1 != null && col2 != null)
                    {
                        var ind1 = table1.Columns.IndexOf(col1);
                        var ind2 = table2.Columns.IndexOf(col2);
                        bool isEnum = false;
                        var col1Type = db.Types.Find(col1.TypeId); 
                        var col2Type = db.Types.Find(col2.TypeId);
                        if (col1Type is TypeEnum && col2Type is TypeEnum)
                            isEnum = true;
                        if (ind1 != -1 && ind2 != -1)
                        {
                            foreach (var row1 in table1.Rows)
                            {
                                var list1 = JsonConvert.DeserializeObject<List<string>>(row1.RowValues);
                                var value1 = list1[ind1];
                                foreach (var row2 in table2.Rows)
                                {
                                    var list2 = JsonConvert.DeserializeObject<List<string>>(row2.RowValues);
                                    var value2 = list2[ind2];
                                    if (isEnum)
                                    {
                                        var enum1 = value1.Split(",");
                                        var enum2 = value2.Split(",");
                                        if (enum1.All(x => enum2.Contains(x)) && enum2.All(x => enum1.Contains(x)))
                                        {
                                            var newRow = new Row();
                                            List<string> list = new List<string>();
                                            list.AddRange(list1);
                                            list.AddRange(list2);
                                            newRow.TableId = table.Id;
                                            newRow.RowValues = JsonConvert.SerializeObject(list);
                                            db.Rows.Add(newRow);
                                        }
                                    }
                                    else if (value1 == value2)
                                    {
                                        var newRow = new Row();
                                        List<string> list = new List<string>();
                                        list.AddRange(list1);
                                        list.AddRange(list2);
                                        newRow.TableId = table.Id;
                                        newRow.RowValues = JsonConvert.SerializeObject(list);
                                        db.Rows.Add(newRow);
                                    }
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
            table.Columns = db.Columns.Where(x => x.TableId == table.Id).ToList();
            table.Rows = db.Rows.Where(x => x.TableId == table.Id).ToList();
            var colToRemove = db.Columns.Where(x => x.TableId == table.Id && x.ColName == request.Field).FirstOrDefault();
            var ind = table.Columns.IndexOf(colToRemove);
            if (ind != -1)
            {
                db.Columns.Remove(colToRemove);
                foreach (var row in table.Rows)
                {
                    var rowlist = JsonConvert.DeserializeObject<List<string>>(row.RowValues);
                    if (rowlist.Count > ind)
                    {
                        rowlist.RemoveAt(ind);
                        row.RowValues = JsonConvert.SerializeObject(rowlist);
                    }
                }
                db.SaveChanges();
            }

            tableReply.Id = table.Id;
            tableReply.Name = table.TableName;
            foreach (var col in table.Columns)
            {
                var type = db.Types.Find(col.TypeId);
                tableReply.Columns.Add(new ColumnReply { ColumnId = col.Id, Name = col.ColName, Type = type.Name });
            }
            foreach (var row in table.Rows)
            {
                var values = JsonConvert.DeserializeObject<List<string>>(row.RowValues);
                List<RowValue> rows = new List<RowValue>();
                RowReply rowReply = new RowReply();
                rowReply.Index = table.Rows.IndexOf(row);
                foreach (var val in values)
                {
                    rowReply.Values.Add(new RowValue { Value = val });
                }
                tableReply.Rows.Add(rowReply);
            }
            return Task.FromResult(tableReply);
        }
    }
}
