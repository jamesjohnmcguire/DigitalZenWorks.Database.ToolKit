using System;
using System.Configuration; 
using System.Data;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;

using Zenware.DiagnosticsLibrary;

namespace Zenware.DatabaseLib
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>DatabaseLibClass</c>
	/// <summary>
	/// Class for Generic database access independent of the underlying 
	/// transport
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class DatabaseLibClass
	{
		/// <summary>
		/// DB_OLEDB
		/// </summary>
		public const uint	DB_OLEDB				= 1;

		/// <summary>
		/// DB_SQLSERVER
		/// </summary>

		public const uint DB_SQLSERVER = 2;

		/// <summary>
		/// DB_ORACLE
		/// </summary>
		public const uint DB_ORACLE = 3;

		/// <summary>
		/// m_nDBType
		/// </summary>
		protected	uint			m_nDBType		= 0;
		/// <summary>
		/// 
		/// </summary>
		protected	string			m_sProvider		= string.Empty;
		/// <summary>
		/// 
		/// </summary>
		protected	string			m_sDataSource	= string.Empty;
		/// <summary>
		/// 
		/// </summary>
		protected	string			m_sCatalog		= string.Empty;
		/// <summary>
		/// 
		/// </summary>
		protected	string			m_sUserId		= string.Empty;
		/// <summary>
		/// 
		/// </summary>
		protected	string			m_sPassword		= string.Empty;
		/// <summary>
		/// 
		/// </summary>
		protected	Diagnostics	m_oDiagnostics	= null;

		// connections
		/// <summary>
		/// OleDb Connection Object
		/// </summary>
		protected	OleDbConnection		m_oOleDbConnection	= null;
		/// <summary>
		/// Sql*  Connection Object
		/// </summary>
		protected	SqlConnection		m_oSqlConnection	= null;
		/// <summary>
		/// Oracle Connection Object
		/// </summary>
		protected	OracleConnection	m_oOracleConnection	= null;


		// transactions
		/// <summary>
		/// 
		/// </summary>
		protected	OleDbTransaction	m_oOleDbTransaction;
		/// <summary>
		/// The main transaction object when using Sql* commands
		/// </summary>
		protected	SqlTransaction		m_oSqlTransaction;
		/// <summary>
		/// The main transaction object when using Oracle* commands
		/// </summary>
		protected	OracleTransaction	m_oOracleTransaction;

		/// <summary>
		/// Prepares all necessary class variables by various constructors
		/// </summary>
		protected void baseConstructor(
			uint nDBType,
			string sApplicationName,
			string sProvider,
			string sDataSource,
			string sCatalog,
			string sUserId,
			string sPassword)
		{
			m_nDBType = nDBType;
			SetDiagnostics( sApplicationName );
			m_oDiagnostics.SetAlertLevel(Diagnostics.ALERT_ERROR | Diagnostics.ALERT_LOGFILE);

			try
			{
				if (sProvider != null)
					m_sProvider		= sProvider;

				if (sDataSource != null)
					m_sDataSource	= sDataSource;

				if (sCatalog != null)
					m_sCatalog		= sCatalog;

				if (sUserId != null)
					m_sUserId		= sUserId;

				if (sPassword != null)
					m_sPassword		= sPassword;

				switch( m_nDBType )
				{
					case DB_OLEDB:
					{
						if ((sProvider != null) && (sDataSource != null))
							m_oOleDbConnection = new OleDbConnection("Provider=" + m_sProvider + ";Data Source=" + m_sDataSource);
						break;
					}
					case DB_SQLSERVER:
					{
						if ((sDataSource != null) && (sCatalog != null))
							m_oSqlConnection  = new SqlConnection("Data Source=" + m_sDataSource + "; " +
								"Integrated Security=SSPI;" +
								"Initial Catalog=" + m_sCatalog );
						break;
					}
					case DB_ORACLE:
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				throw( ex );
			}
			finally
			{
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public DatabaseLibClass()
		{
			baseConstructor( DB_OLEDB, "DBLib", null, null, null, null, null);
		}

		/// <summary>
		/// Database type constructor
		/// </summary>
		/// <param name="nDBType"></param>
		public DatabaseLibClass(
			uint nDBType)
		{
			baseConstructor( nDBType, "DBLib", null, null, null, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sApplicationName"></param>
		public DatabaseLibClass(
			string sApplicationName)
		{
			baseConstructor( DB_OLEDB, sApplicationName, null, null, null, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nDBType"></param>
		/// <param name="sApplicationName"></param>
		public DatabaseLibClass(
			uint nDBType,
			string sApplicationName)
		{
			baseConstructor( nDBType, sApplicationName, null, null, null, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sProvider"></param>
		/// <param name="sDataSource"></param>
		public DatabaseLibClass(
			string sProvider,
			string sDataSource)
		{
			baseConstructor( DB_OLEDB, "DBLib", sProvider, sDataSource, null, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sApplicationName"></param>
		/// <param name="sProvider"></param>
		/// <param name="sDataSource"></param>
		public DatabaseLibClass(
			string sApplicationName,
			string sProvider,
			string sDataSource)
		{
			baseConstructor( DB_OLEDB, sApplicationName, sProvider, sDataSource, null, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nDBType"></param>
		/// <param name="sDataSource"></param>
		/// <param name="sCatalog"></param>
		public DatabaseLibClass(
			uint nDBType,
			string sDataSource,
			string sCatalog)
		{
			baseConstructor( nDBType, "DBLib", null, sDataSource, sCatalog, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nDBType"></param>
		/// <param name="sDataSource"></param>
		/// <param name="sUserId"></param>
		/// <param name="sPassword"></param>
		public DatabaseLibClass(
			uint nDBType,
			string sDataSource,
			string sUserId,
			string sPassword)
		{
			baseConstructor( nDBType, "DBLib", null, sDataSource, null, sUserId, sPassword);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nDBType"></param>
		/// <param name="sApplicationName"></param>
		/// <param name="sDataSource"></param>
		/// <param name="sCatalog"></param>
		/// <param name="sUserId"></param>
		/// <param name="sPassword"></param>
		public DatabaseLibClass(
			uint nDBType,
			string sApplicationName,
			string sDataSource,
			string sCatalog,
			string sUserId,
			string sPassword)
		{
			baseConstructor( nDBType, sApplicationName, null, sDataSource, sCatalog, sUserId, sPassword);
		}

		/// <summary>
		/// Closes the database connection and object
		/// </summary>
		public void Close()
		{
			switch (m_nDBType)
			{
				case DB_OLEDB:
				{
					if(m_oOleDbConnection != null)
					{
						if(m_oOleDbConnection.State != ConnectionState.Closed)
							m_oOleDbConnection.Close();		
					}
					break;
				}
				case DB_SQLSERVER:
				{
					if(m_oSqlConnection != null)
					{
						if(m_oSqlConnection.State != ConnectionState.Closed)
							m_oSqlConnection.Close();		
					}
					break;
				}
				case DB_ORACLE:
				{
					if(m_oOracleConnection != null)
					{
						if(m_oOracleConnection.State != ConnectionState.Closed)
							m_oOracleConnection.Close();		
					}
					break;
				}
			}
			System.GC.Collect();
		}

		/// <summary>
		/// Establishes connection with the database.
		/// </summary>
		public void EstablishConnection()
		{
			try
			{
				switch (m_nDBType)
				{
					case DB_OLEDB:
					{
						if (null == m_oOleDbConnection)
						{
							if ((m_sProvider != null) && (m_sDataSource != null))
								m_oOleDbConnection = new OleDbConnection("Provider=" + m_sProvider + ";Data Source=" + m_sDataSource);
						}

						if (m_oOleDbConnection.State != ConnectionState.Open)
							m_oOleDbConnection.Open( );

						break;
					}
					case DB_SQLSERVER:
					{
						if (null == m_oSqlConnection)
						{
							if ((m_sDataSource != null) && (m_sCatalog != null))
								m_oSqlConnection  = new SqlConnection("Data Source=" + m_sDataSource + "; " +
									"Integrated Security=SSPI;" +
									"Initial Catalog=" + m_sCatalog );
						}

						if (m_oSqlConnection.State != ConnectionState.Open)
							m_oSqlConnection.Open( );

						break;
					}
					case DB_ORACLE:
					{
						if (null == m_oOracleConnection)
						{
							if ((m_sDataSource != null) && (m_sUserId != null) && (m_sPassword != null))
							{
								string	sConnectionString	= "Data Source=" +
									m_sDataSource +
									";User Id=" +
									m_sUserId +
									";Password=" +
									m_sPassword +
									";";
								m_oOracleConnection = new OracleConnection(sConnectionString);
							}
						}

						if (m_oOracleConnection.State != ConnectionState.Open)
							m_oOracleConnection.Open( );

						break;
					}
				}
			}
			catch (Exception ex)
			{
				m_oDiagnostics.SetEvent( "EstablishConnection - DB connect problem. Exception: " +
					ex.Message,
					Diagnostics.ALERT_ERROR);
			}
		}

		/// <summary>
		/// This opens a connection and begin's the transaction.
		/// </summary>
		public void BeginTransaction()
		{
			EstablishConnection();

			switch (m_nDBType)
			{
				case DB_OLEDB:
				{
					m_oOleDbTransaction = m_oOleDbConnection.BeginTransaction();
					break;
				}
				case DB_SQLSERVER:
				{
					m_oSqlTransaction = m_oSqlConnection.BeginTransaction();
					break;
				}
				case DB_ORACLE:
				{
					m_oOracleTransaction = m_oOracleConnection.BeginTransaction();
					break;
				}
			}
		}

		/// <summary>
		/// This closes the transaction.
		/// </summary>
		public void CloseTransaction()
		{
			switch (m_nDBType)
			{
				case DB_OLEDB:
				{
					//m_oOleDbTransaction.Connection.Close();
					m_oOleDbTransaction = null;
					break;
				}
				case DB_SQLSERVER:
				{
					m_oSqlTransaction.Connection.Close();
					m_oSqlTransaction = null;
					break;
				}
				case DB_ORACLE:
				{
					m_oOracleTransaction.Connection.Close();
					m_oOracleTransaction = null;
					break;
				}
			}
		}

		/// <summary>
		/// This commits the transaction.
		/// </summary>
		public void CommitTansaction()
		{
			switch (m_nDBType)
			{
				case DB_OLEDB:
				{
					m_oOleDbTransaction.Commit();
					break;
				}
				case DB_SQLSERVER:
				{
					m_oSqlTransaction.Commit();
					break;
				}
				case DB_ORACLE:
				{
					m_oOracleTransaction.Commit();
					break;
				}
			}
		}

		/// <summary>
		/// This rollsback the transaction.
		/// </summary>
		public void RollBackTransaction()
		{
			switch (m_nDBType)
			{
				case DB_OLEDB:
				{
					m_oOleDbTransaction.Rollback();
					break;
				}
				case DB_SQLSERVER:
				{
					m_oSqlTransaction.Rollback();
					break;
				}
				case DB_ORACLE:
				{
					m_oOracleTransaction.Rollback();
					break;
				}
			}
		}

		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sQuery"></param>
		/// <param name="oDataSet"></param>
		/// <returns></returns>
		public int GetDataSet(
			string sQuery,
			out DataSet oDataSet)
		{
			int		nRows			= -1;

			oDataSet			= new DataSet();

			try
			{
				EstablishConnection();

				switch (m_nDBType)
				{
					case DB_OLEDB:
					{
						OleDbCommand	oOleDbCommand;

						if (m_oOleDbTransaction != null)
							oOleDbCommand = new OleDbCommand(
								sQuery, 
								m_oOleDbConnection, 
								m_oOleDbTransaction);
						else
							oOleDbCommand = new OleDbCommand(
								sQuery, 
								m_oOleDbConnection);

						OleDbDataAdapter	oDataAdapter = new OleDbDataAdapter();
						oDataAdapter.SelectCommand	= oOleDbCommand;

						nRows	= oDataAdapter.Fill(oDataSet);
						break;
					}
					case DB_SQLSERVER:
					{
						SqlCommand	oSqlCommand;

						if (m_oSqlTransaction != null)
							oSqlCommand = new SqlCommand(
								sQuery, 
								m_oSqlConnection, 
								m_oSqlTransaction);
						else
							oSqlCommand = new SqlCommand(
								sQuery, 
								m_oSqlConnection);

						oSqlCommand.CommandTimeout = 30;

						SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
						sqlDataAdapter.SelectCommand = oSqlCommand;

						nRows	= sqlDataAdapter.Fill(oDataSet);
						break;
					}
					case DB_ORACLE:
					{
						OracleCommand	oOracleCommand;

						if (m_oOracleTransaction != null)
							oOracleCommand = new OracleCommand(
								sQuery, 
								m_oOracleConnection, 
								m_oOracleTransaction);
						else
							oOracleCommand = new OracleCommand(
								sQuery, 
								m_oOracleConnection);

						OracleDataAdapter	oDataAdapter = new OracleDataAdapter();
						oDataAdapter.SelectCommand	= oOracleCommand;

						nRows	= oDataAdapter.Fill(oDataSet);
						break;
					}
				}
				m_oDiagnostics.SetEvent( "OK - getDataSet - Query: " + sQuery);
			}
			catch (Exception ex)
			{
				m_oDiagnostics.SetEvent( "getDataSet - DB connect problem. Exception: " +
					ex.Message +
					" - Query: " + sQuery,
					Diagnostics.ALERT_ERROR);
			}
			finally
			{
				Close( );
			}

			return nRows;
		}

		/// <summary>
		/// Prepares and executes a Non-Query DB Command
		/// </summary>
		public bool ExecuteNonQuery(string SqlQueryCommand)
		{
			bool ReturnCode = false;

			OleDbCommand OleDbCommandObject = null;
			SqlCommand SqlCommandObject = null;
			OracleCommand oOracleCommand = null;

			try
			{
				switch (m_nDBType)
				{
					case DB_OLEDB:
					{
						if (m_oOleDbTransaction != null)
							OleDbCommandObject = new OleDbCommand(
								SqlQueryCommand, 
								m_oOleDbConnection, 
								m_oOleDbTransaction);
						else
							OleDbCommandObject = new OleDbCommand(
								SqlQueryCommand,
								m_oOleDbConnection);

						// Set timeout to 1 minute
						OleDbCommandObject.CommandTimeout = 60;

						if (m_oOleDbConnection.State == ConnectionState.Closed)
						{
							m_oOleDbConnection.Open();
						}

						int RowsEffected = OleDbCommandObject.ExecuteNonQuery();

						if (RowsEffected > 0)
						{
							ReturnCode = true;
						}
						break;
					}
					case DB_SQLSERVER:
					{
						if (m_oSqlTransaction != null)
						{
							SqlCommandObject = new SqlCommand(
								SqlQueryCommand,
								m_oSqlConnection,
								m_oSqlTransaction);
						}
						else
						{
							SqlCommandObject = new SqlCommand(
								SqlQueryCommand,
								m_oSqlConnection);
						}

						// Set timeout to 1 minute
						SqlCommandObject.CommandTimeout = 60;

						if (m_oSqlConnection.State == ConnectionState.Closed)
						{
							m_oSqlConnection.Open();
						}

						int RowsEffected = SqlCommandObject.ExecuteNonQuery();

						if (RowsEffected > 0)
						{
							ReturnCode = true;
						}
						break;
					}
					case DB_ORACLE:
					{
						if (m_oOracleTransaction != null)
							oOracleCommand = new OracleCommand(
								SqlQueryCommand,
								m_oOracleConnection, 
								m_oOracleTransaction);
						else
							oOracleCommand = new OracleCommand(
								SqlQueryCommand,
								m_oOracleConnection);

						// Set timeout to 1 minute
						oOracleCommand.CommandTimeout = 60;

						if (m_oOracleConnection.State == ConnectionState.Closed)
						{
							m_oOracleConnection.Open();
						}

						int RowsEffected = oOracleCommand.ExecuteNonQuery();

						if (RowsEffected > 0)
						{
							ReturnCode = true;
						}
						break;
					}
				}
			}
			catch(Exception exNonQuery)
			{
				SetExceptionError(	
					exNonQuery, 
					"ExecuteNonQuery - An exception was encountered while attempting to execute the command. Exception: ",
					SqlQueryCommand);
				try
				{
					RollBackTransaction();
					Close();
				}
				catch (OleDbException exRollBack)
				{
					SetExceptionError(	
						exRollBack, 
						"ExecuteNonQuery - An exception was encountered while attempting to roll back the transaction. Exception: ",
						SqlQueryCommand);
				}
				catch (SqlException exRollBack)
				{
					SetExceptionError(	
						exRollBack, 
						"ExecuteNonQuery - An exception was encountered while attempting to roll back the transaction. Exception: ",
						SqlQueryCommand);
				}
				catch (OracleException exRollBack)
				{
					SetExceptionError(	
						exRollBack, 
						"ExecuteNonQuery - An exception was encountered while attempting to roll back the transaction. Exception: ",
						SqlQueryCommand);
				}
				catch (Exception exRollBack)
				{
					SetExceptionError(	
						exRollBack, 
						"ExecuteNonQuery - An exception was encountered while attempting to roll back the transaction. Exception: ",
						SqlQueryCommand);
				}
				throw;
			}
			finally 
			{
				switch (m_nDBType)
				{
					case DB_OLEDB:
					{
						if (m_oOleDbTransaction == null)
							Close();
						break;
					}
					case DB_SQLSERVER:
					{
						if (m_oSqlTransaction == null)
							Close();
						if (SqlCommandObject != null)
						{
							SqlCommandObject.Dispose();
						}
						break;
					}
					case DB_ORACLE:
					{
						if (m_oOracleTransaction == null)
							Close();
						break;
					}
				}
			}
			return ReturnCode;
		}


		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prepares and executes a Non-Query DB Command
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		protected int ExecuteScalar(string SqlQueryCommand)
		{
			int Result = -1;

			OleDbCommand OleDbCommandObject = null;
			SqlCommand SqlCommandObject = null;
			OracleCommand oOracleCommand = null;

			try
			{
				switch (m_nDBType)
				{
					case DB_OLEDB:
						{
							if (m_oOleDbTransaction != null)
								OleDbCommandObject = new OleDbCommand(
									SqlQueryCommand,
									m_oOleDbConnection,
									m_oOleDbTransaction);
							else
								OleDbCommandObject = new OleDbCommand(
									SqlQueryCommand,
									m_oOleDbConnection);

							// Set timeout to 1 minute
							OleDbCommandObject.CommandTimeout = 60;

							if (m_oOleDbConnection.State == ConnectionState.Closed)
							{
								m_oOleDbConnection.Open();
								m_oOleDbConnection.Open();
							}

							Result = (int)OleDbCommandObject.ExecuteScalar();

							break;
						}
					case DB_SQLSERVER:
						{
							if (m_oSqlTransaction != null)
							{
								SqlCommandObject = new SqlCommand(
									SqlQueryCommand,
									m_oSqlConnection,
									m_oSqlTransaction);
							}
							else
							{
								SqlCommandObject = new SqlCommand(
									SqlQueryCommand,
									m_oSqlConnection);
							}

							// Set timeout to 1 minute
							SqlCommandObject.CommandTimeout = 60;

							if (m_oSqlConnection.State == ConnectionState.Closed)
							{
								m_oSqlConnection.Open();
							}

							Result = (int)SqlCommandObject.ExecuteScalar();

							break;
						}
					case DB_ORACLE:
						{
							if (m_oOracleTransaction != null)
								oOracleCommand = new OracleCommand(
									SqlQueryCommand,
									m_oOracleConnection,
									m_oOracleTransaction);
							else
								oOracleCommand = new OracleCommand(
									SqlQueryCommand,
									m_oOracleConnection);

							// Set timeout to 1 minute
							oOracleCommand.CommandTimeout = 60;

							if (m_oOracleConnection.State == ConnectionState.Closed)
							{
								m_oOracleConnection.Open();
							}

							Result = (int)oOracleCommand.ExecuteScalar();
							break;
						}
				}
			}
			catch (Exception exNonQuery)
			{
				SetExceptionError(
					exNonQuery,
					"ExecuteNonQuery - An exception was encountered while attempting to execute the command. Exception: ",
					SqlQueryCommand);
				try
				{
					RollBackTransaction();
					Close();
				}
				catch (OleDbException exRollBack)
				{
					SetExceptionError(
						exRollBack,
						"ExecuteNonQuery - An exception was encountered while attempting to roll back the transaction. Exception: ",
						SqlQueryCommand);
				}
				catch (SqlException exRollBack)
				{
					SetExceptionError(
						exRollBack,
						"ExecuteNonQuery - An exception was encountered while attempting to roll back the transaction. Exception: ",
						SqlQueryCommand);
				}
				catch (OracleException exRollBack)
				{
					SetExceptionError(
						exRollBack,
						"ExecuteNonQuery - An exception was encountered while attempting to roll back the transaction. Exception: ",
						SqlQueryCommand);
				}
				catch (Exception exRollBack)
				{
					SetExceptionError(
						exRollBack,
						"ExecuteNonQuery - An exception was encountered while attempting to roll back the transaction. Exception: ",
						SqlQueryCommand);
				}
				throw;
			}
			finally
			{
				switch (m_nDBType)
				{
					case DB_OLEDB:
						{
							if (m_oOleDbTransaction == null)
								Close();
							break;
						}
					case DB_SQLSERVER:
						{
							if (m_oSqlTransaction == null)
								Close();
							if (SqlCommandObject != null)
							{
								SqlCommandObject.Dispose();
							}
							break;
						}
					case DB_ORACLE:
						{
							if (m_oOracleTransaction == null)
								Close();
							break;
						}
				}
			}
			return Result;
		}

		/// <summary>
		/// Sets the level for diagnostics notification.
		/// </summary>
		/// <param name="sApplicationName"></param>
		/// <returns></returns>
		public bool SetDiagnostics(string sApplicationName)
		{
			bool	bInit	= false;
			try
			{
				m_oDiagnostics	= new Diagnostics( sApplicationName, "DBLib" );

				bInit	= true;
			}
			catch
			{
				bInit	= false;
			}

			return bInit;
		}

		public void ShutDown()
		{
			Close( );
			switch (m_nDBType)
			{
				case DB_OLEDB:
				{
					if(m_oOleDbConnection != null)
					{
						m_oOleDbConnection.Dispose();
						m_oOleDbConnection = null;
					}
					break;
				}
				case DB_SQLSERVER:
				{
					if(m_oSqlConnection != null)
					{
						m_oSqlConnection.Dispose();
						m_oSqlConnection = null;
					}
					break;
				}
				case DB_ORACLE:
				{
					if(m_oOracleConnection != null)
					{
						m_oOracleConnection.Dispose();
						m_oOracleConnection = null;
					}
					break;
				}
			}
			System.GC.Collect();
		}

		public void SetExceptionError(Exception ex, string sIntroMsg, string sCommand)
		{
			m_oDiagnostics.SetEvent( 
				sIntroMsg + 
				ex.Message +
				" - Command: " + 
				sCommand,
				Diagnostics.ALERT_ERROR);
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>IsConnected</c>
		/// <summary>
		/// Checks to see if the database is open and connected. Helper function
		/// for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool IsConnected()
		{
			bool Connected = false;

			if (m_oSqlConnection.State == ConnectionState.Open)
			{
				Connected = true;
			}

			return Connected;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>CanQuery</c>
		/// <summary>
		/// Checks to see if the database can return a valid query. Helper
		/// function for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool CanQuery()
		{
			bool CanQuery = false;

			DataSet TestDataSet = null;
			int RecordsReturned = -1;

			string SqlQueryCommand = "SELECT @@VERSION";
			RecordsReturned = GetDataSet(SqlQueryCommand, out TestDataSet);

			if (RecordsReturned > 0)
			{
				CanQuery = true;
			}

			return CanQuery;
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// GetDataTable
		/// </summary>
		/// <param name="SqlQueryCommand"></param>
		/// <param name="ReturnedDataTable"> (for returning data)</param>
		/// <returns>number of records retrieved</returns>
		/////////////////////////////////////////////////////////////////////////
		public int GetDataTable(string SqlQueryCommand, out DataTable ReturnedDataTable)
		{
			int RecordsReturned = -1;
			DataSet dtTempSet = null;

			ReturnedDataTable = new DataTable();

			RecordsReturned = GetDataSet(SqlQueryCommand, out dtTempSet);
			if (dtTempSet.Tables.Count > 0)
			{
				ReturnedDataTable = dtTempSet.Tables[0];
			}

			return RecordsReturned;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataRow</c>
		/// <summary>
		/// Gets a single row of data
		/// </summary>
		/// <param name="SqlCommandQuery"></param>
		/// <param name="DataRowOut"> (for returning data)</param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool GetDataRow(string SqlCommandQuery, out DataRow DataRowOut)
		{
			bool HasData = false;
			int RecordsReturned = -1;
			DataTable TempDataTable = null;

			DataRowOut = null;

			RecordsReturned = GetDataTable(SqlCommandQuery, out TempDataTable);

			if (RecordsReturned > 0)
			{
				DataRowOut = TempDataTable.Rows[0];
				HasData = true;
			}

			return HasData;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>UpdateCommand</c>
		/// <summary>
		/// Performs an Sql UPDATE command
		/// </summary>
		/// <param name="SqlCommandQuery"></param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool UpdateCommand(string SqlQueryCommand)
		{
			bool ReturnCode = ExecuteNonQuery(SqlQueryCommand);

			return ReturnCode;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>InsertCommand</c>
		/// <summary>
		/// Performs an Sql UPDATE command
		/// </summary>
		/// <param name="SqlQueryCommand"></param>
		/// <returns>object item</returns>
		/////////////////////////////////////////////////////////////////////////
		public int InsertCommand(string SqlQueryCommand)
		{
			int ReturnCode = ExecuteScalar(SqlQueryCommand);

			return ReturnCode;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>DeleteCommand</c>
		/// <summary>
		/// Performs an Sql DELETE command
		/// </summary>
		/// <param name="SqlQueryCommand"></param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool DeleteCommand(string SqlQueryCommand)
		{
			bool ReturnCode = ExecuteNonQuery(SqlQueryCommand);

			return ReturnCode;
		}
	}	// end class
}	// end namespace
