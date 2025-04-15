# xyDb
Definition of general database operation interface and implementation of specific database operations for each database  
  
Currently includes: SQLite64, PostgreSQL, SqlServer  

## Sample
Please read code details in project xyDbSample.  
### Create DbService
IDbAccess dbAccess = new SQLite64DbAccess();  
//IDbAccess dbAccess = new PostgreSQLDbAccess();  
//IDbAccess dbAccess = new SQLServerDbAccess();  
string ConnectionString = @"Data Source=testDb;";  
//string ConnectionString = @"Server=localhost;Database=testdb;User Id=testuser;Password=testPassword;";  
//string ConnectionString = @"Server=localhost\\SQLEXPRESS;uid=testUser;pwd=testPassword;database=testDb;Packet Size=8192;Max Pool Size=1000;Connect Timeout=30;";  
DbService = new DbService(ConnectionString, dbAccess);  
await DbService.openAsync();  
### New record and query
maxId++;  
string sql = "INSERT INTO COMPANY(ID, NAME, AGE) " + "VALUES(" + maxId + ", '', 0)";  
await dbService.exeSqlAsync(sql);  
sql = "SELECT * From COMPANY WHERE ID=" + maxId;  
DataTable dt = await dbService.exeSqlForDataSetAsync(sql);  
int i = dataGridView1.Rows.Add(dt.Rows[0].ItemArray);  
dataGridView1.Rows[i].Tag = dt.Rows[0];  
### Edit record
string colName = dataGridView1.Columns[e.ColumnIndex].Name;  
string cellValue = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();  
string rowID =  dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString();  
string sql = "UPDATE COMPANY SET " + colName + "='" + cellValue + "' "  + " WHERE ID=" + rowID;  
await dbService.exeSqlAsync(sql);  
### Delete record
int rowIndex = dataGridView1.SelectedRows[0].Index;  
int rowID = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["ID"].Value);  
string sql = "DELETE FROM COMPANY " + " WHERE ID=" + rowID;  
await dbService.exeSqlAsync(sql);  
dataGridView1.Rows.RemoveAt(rowIndex);  
