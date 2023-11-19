using Grpc.Net.Client;
using Service;

using var channel = GrpcChannel.ForAddress("https://localhost:7294");

var client = new DBService.DBServiceClient(channel);

int dbId = 0;
dbId = ChooseDBAction();

int ChooseDBAction()
{
    Console.WriteLine("\nChoose the command: \n1. open DB \n2. create DB \n3. exit");
    string command = Console.ReadLine();
    switch (command)
    {
        case "1":
            Console.WriteLine("\n1.id or 2.path"); 
            string com = Console.ReadLine();
            if (com == "1")
            {
                DBsReply result = client.GetDBs(new Google.Protobuf.WellKnownTypes.Empty());
                Console.WriteLine("DBs: ");
                foreach (var db in result.DBs)
                {
                    Console.WriteLine($"DB id: {db.Id}, DB name: {db.Name}");
                }
                Console.WriteLine("Enter DB id: ");
                dbId = int.Parse(Console.ReadLine()); 
                ChooseTablesAction(dbId);
            }
            else if(com == "2")
            {
                Console.WriteLine("Enter DB path: ");
                string path = Console.ReadLine();
                DBReply result = client.OpenDB(new OpenDBRequest { Path = path });
                Console.WriteLine((result.Id == 0 ? "" : ("DB id: " + result.Id + ", ")) + "DB name: " + result.Name + ", tables count: " + result.TablesCount + "\n");
                if (result.Id == 0)
                    ChooseDBAction();
                else
                    ChooseTablesAction(dbId);
            }
            break;
        case "2":
            Console.WriteLine("Enter DB name: ");
            var name = Console.ReadLine();
            CreateDBReply createDBReply = client.CreateDB(new CreateDBRequest { Name = name });
            dbId = createDBReply.Id;
            ChooseTablesAction(dbId);
            break;
        case "3":
            Environment.Exit(0);
            break;
    }
    return dbId;
}

void ChooseTablesAction(int dbId)
{
    Console.WriteLine("\nChoose the command: \n1. show tables \n2. create table \n3. save db \n4. back \n5. exit");
    string command = Console.ReadLine();
    switch (command)
    {
        case "1":
            TablesReply tablesReply = client.GetTables(new GetTablesRequest { DbId = dbId });
            Console.WriteLine("DB tables: " + tablesReply.Tables.Count);
            foreach (var table in tablesReply.Tables)
                Console.WriteLine("Table id: " + table.Id + "\nTable name: " + table.Name + "\n");
            ChooseTableAction(dbId);
            break;
        case "2":
            Console.WriteLine("Enter table name: ");
            var name = Console.ReadLine();
            ResultReply result = client.AddTable(new CreateTableRequest { Name = name, DbId = dbId });
            Console.WriteLine(result.Result ? "Success!\n" : "Failure\n");
            TablesReply allTables = client.GetTables(new GetTablesRequest { DbId = dbId });
            Console.WriteLine("DB tables: " + allTables.Tables.Count);
            foreach (var table in allTables.Tables)
                Console.WriteLine("Table id: " + table.Id + "\nTable name: " + table.Name + "\n");
            ChooseTableAction(dbId);
            break;
        case "3":
            ResultReply res = client.SaveDB(new SaveDBRequest { Id = dbId } );
            Console.WriteLine(res.Result ? "Success!\n" : "Failure\n");
            ChooseDBAction();
            break;
        case "4":
            ChooseDBAction();
            break;
        case "5":
            Environment.Exit(0);
            break;
    }
}

void ChooseTableAction(int dbId)
{
    Console.WriteLine("\nChoose the command: \n1. edit table \n2. delete table \n3. union of tables \n4. back \n5. exit");
    string command = Console.ReadLine();
    switch (command)
    {
        case "1":
            Console.WriteLine("Enter table id: ");
            int tableId = int.Parse(Console.ReadLine());
            TableReply tableReply = client.GetTable(new GetTableRequest { TableId = tableId });
            ShowTable(tableReply);
            ChooseTableModifyCommand(tableReply);
            break;
        case "2":
            Console.WriteLine("Enter table id: ");
            int tableid = int.Parse(Console.ReadLine());
            ResultReply result = client.DeleteTable(new DeleteTableRequest { TableId = tableid });
            Console.WriteLine(result.Result ? "Success!\n" : "Failure\n");

            TablesReply allTables = client.GetTables(new GetTablesRequest { DbId = dbId });
            Console.WriteLine("DB tables: " + allTables.Tables.Count);
            foreach (var table in allTables.Tables)
                Console.WriteLine("Table id: " + table.Id + "\nTable name: " + table.Name + "\n");
            ChooseTableAction(dbId);
            break;
        case "3":
            TablesReply tablesReply = client.GetTables(new GetTablesRequest { DbId = dbId });
            Console.WriteLine("DB tables: " + tablesReply.Tables.Count);
            foreach (var table in tablesReply.Tables)
                Console.WriteLine("Table id: " + table.Id + "\nTable name: " + table.Name + "\n");

            Console.WriteLine("Enter table1 id: ");
            int table1id = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter table2 id: ");
            int table2id = int.Parse(Console.ReadLine());

            TablesFieldReply fields = client.GetTablesField(new GetTablesFieldRequest { Id1 = table1id, Id2 = table2id });
            Console.WriteLine("Fields: ");
            foreach (var f in fields.Fields)
            {
                Console.WriteLine(f.Field);
            }
            Console.WriteLine("Enter field name: "); 
            string fieldName = Console.ReadLine();
            TableReply union = client.UnionOfTables(new UnionOfTablesRequest { Id1 = table1id, Id2 = table2id,Field = fieldName });
            ShowTable(union);
            ChooseTableAction(dbId);
            break;
        case "4":
            ChooseTablesAction(dbId);
            break;
        case "5":
            Environment.Exit(0);
            break;
    }
}

void ChooseTableModifyCommand(TableReply tableReply)
{
    bool hasColumns = tableReply.Columns.Count > 0;
    bool hasRows = tableReply.Rows.Count > 0;
    if (hasColumns && hasRows)
        Console.WriteLine("\nChoose the command: \n1. add column \n2. add row \n3. modify row \n4. back \n5. exit");
    else if (hasColumns && !hasRows)
        Console.WriteLine("\nChoose the command: \n1. add column \n2. add row \n4. back \n5. exit\n");
    else
        Console.WriteLine("\nChoose the command: \n1. add column \n4. back \n5. exit\n");
    string command = Console.ReadLine();
    switch (command)
    {
        case "1":
            Console.WriteLine("Types:\n");
            TypesReply typesReply = client.GetTypes(new Google.Protobuf.WellKnownTypes.Empty());
            foreach (var type in typesReply.Types_)
                Console.WriteLine("Type id: " + type.Id + "\nType name: " + type.Name + "\n");

            Console.WriteLine("Enter type id: ");
            var typeId = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter column name: ");
            var columnName = Console.ReadLine();

            ResultReply resultReply = client.AddColumn(new AddColumnRequest { TableId = tableReply.Id, Name = columnName, TypeId = typeId });
            Console.WriteLine(resultReply.Result ? "Success!\n" : "Failure\n");
            TableReply updTable = client.GetTable(new GetTableRequest { TableId = tableReply.Id });
            ShowTable(updTable);
            ChooseTableModifyCommand(updTable);
            break;
        case "2":
            ResultReply result = client.AddRow(new AddRowRequest { TableId = tableReply.Id });
            Console.WriteLine(result.Result ? "Success!\n" : "Failure\n");
            TableReply updTableResult = client.GetTable(new GetTableRequest { TableId = tableReply.Id });
            ShowTable(updTableResult);
            ChooseTableModifyCommand(updTableResult);
            break;
        case "3":
            ShowTable(tableReply);
            Console.WriteLine("Enter column id: ");
            int colId = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter row index: ");
            int rowIndex = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter new value: ");
            string newValue = Console.ReadLine();
            ResultReply res = client.ChangeValue(new ChangeValueRequest { TableId = tableReply.Id, ColumnId = colId, RIndex = rowIndex, NewValue = newValue });
            Console.WriteLine(res.Result ? "Success!\n" : "Failure\n");
            TableReply updTableresult = client.GetTable(new GetTableRequest { TableId = tableReply.Id });
            ShowTable(updTableresult);
            
            ChooseTableModifyCommand(updTableresult);
            break;
        case "4":
            ChooseTableAction(dbId);
            break;
        case "5":
            Environment.Exit(0);
            break;
    }
}


void ShowTable(TableReply tableReply)
{
    Console.WriteLine("Table: " + tableReply.Name + "\n");
    foreach (var column in tableReply.Columns)
    {
        Console.Write($"{column.Name} ({column.Type})".PadRight(20));
    }
    Console.WriteLine();
    foreach (var column in tableReply.Columns)
    {
        Console.Write($"id: {column.ColumnId}".PadRight(20));
    }
    Console.WriteLine();
    foreach (var row in tableReply.Rows)
    {
        Console.Write($"index: {row.Index}".PadRight(10));
        Console.WriteLine();
        foreach (var value in row.Values)
        {
            Console.Write($"{value.Value}".PadRight(20));
        }
        Console.WriteLine();
    }
}