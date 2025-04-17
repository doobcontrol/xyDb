# xy Db SQLite64 Implement
xy.Db is a definition of general database operation interface and implementation of specific database operations for each database, Currently includes: SQLite64, PostgreSQL, SqlServer, MySql  
This project is a SQLite64 implementation of xy.Db.
## Sample
Please read code details in project xyDbSample.  
### Create DbService
IDbAccess dbAccess = new SQLite64DbAccess();  
string ConnectionString = @"Data Source=testDb;";  
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
