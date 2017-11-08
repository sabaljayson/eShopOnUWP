﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace eShop.SqlProvider
{
    public class SqlServerProvider
    {
        const string QUERY_TYPES = "SELECT * FROM CatalogTypes";
        const string QUERY_BRANDS = "SELECT * FROM CatalogBrands";
        const string QUERY_ITEMS = "SELECT [Id], [Name], [Description], [Price], [CatalogTypeId], [CatalogBrandId], [PictureFileName] FROM CatalogItems";
        const string QUERY_ITEMSBYID = "SELECT [Id], [Name], [Description], [Price], [CatalogTypeId], [CatalogBrandId] FROM CatalogItems WHERE Id=@Id";
        const string QUERY_IMAGEBYID = "SELECT [Id], [ImageType], [ImageBytes] FROM CatalogImages WHERE Id=@Id";

        const string CREATE_TYPES = "INSERT INTO CatalogTypes ([Id], [Type]) VALUES (@Id, @Type)";
        const string CREATE_BRANDS = "INSERT INTO CatalogBrands ([Id], [Brand]) VALUES (@Id, @Brand)";
        const string CREATE_ITEMS = "INSERT INTO CatalogItems ([Name], [Description], [Price], [CatalogTypeId], [CatalogBrandId]) VALUES (@Name, @Description, @Price, @CatalogTypeId, @CatalogBrandId) SET @Id = SCOPE_IDENTITY()";
        const string UPDATE_ITEMS = "UPDATE CatalogItems SET [Name] = @Name, [Description] = @Description, [Price] = @Price, [CatalogTypeId] = @CatalogTypeId, [CatalogBrandId] = @CatalogBrandId, [PictureFileName] = @PictureFileName WHERE [Id] = @Id";
        const string DELETE_ITEM = "DELETE FROM CatalogItems WHERE [Id] = @Id";
        const string CREATE_IMAGE = "INSERT INTO CatalogImages ([Id], [ImageType], [ImageBytes]) VALUES (@Id, @ImageType, @ImageBytes)";

        const string QUERY_EXISTSDB = "SELECT count(*) FROM sys.Databases WHERE name = @DbName";

        public SqlServerProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; private set; }

        public bool DatabaseExists()
        {
            SqlConnectionStringBuilder cnnStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
            string dbName = cnnStringBuilder.InitialCatalog;
            cnnStringBuilder.InitialCatalog = "master";
            string masterConnectionString = cnnStringBuilder.ConnectionString;

            using (SqlConnection cnn = new SqlConnection(masterConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(QUERY_EXISTSDB, cnn))
                {
                    SqlParameter param = new SqlParameter("DbName", dbName);
                    cmd.Parameters.Add(param);
                    return (int)cmd.ExecuteScalar() == 1;
                }
            }
        }

        public void CreateDatabase()
        {
            SqlConnectionStringBuilder cnnStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
            string dbName = cnnStringBuilder.InitialCatalog;
            if (dbName == null)
            {
                throw new ArgumentNullException("Initial Catalog");
            }
            if (dbName.Equals("master", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid Initial Catalog 'master'.");
            }
            cnnStringBuilder.InitialCatalog = "master";
            string masterConnectionString = cnnStringBuilder.ConnectionString;

            using (SqlConnection cnn = new SqlConnection(masterConnectionString))
            {
                cnn.Open();
                foreach (string sqlLine in GetSqlScriptLines(dbName))
                {
                    using (SqlCommand cmd = new SqlCommand(sqlLine, cnn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private IEnumerable<string> GetSqlScriptLines(string dbName)
        {
            string sqlScript = GetSqlScript();
            sqlScript = sqlScript.Replace("[DATABASE_NAME]", dbName);
            using (var reader = new StringReader(sqlScript))
            {
                string sql = "";
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Trim() == "GO")
                    {
                        yield return sql;
                        sql = "";
                    }
                    else
                    {
                        sql += line;
                    }
                    line = reader.ReadLine();
                }
            }
        }

        private string GetSqlScript()
        {
            Stream stream = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("eShop.SqlProvider.SqlScripts.CreateDb.sql");
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public DataSet GetDatasetSchema()
        {
            string sqlQuery = QUERY_TYPES + " WHERE 1=0; " + QUERY_BRANDS + " WHERE 1=0; " + QUERY_ITEMS + " WHERE 1=0";

            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    DataSet dataSet = new DataSet();
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                    {
                        dataAdapter.Fill(dataSet);
                        dataSet.Tables[0].TableName = "CatalogTypes";
                        dataSet.Tables[1].TableName = "CatalogBrands";
                        dataSet.Tables[2].TableName = "CatalogItems";
                    }
                    return dataSet;
                }
            }
        }

        public DataSet GetCatalogTypes()
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(QUERY_TYPES, cnn))
                {
                    DataSet dataSet = new DataSet();
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                    {
                        dataAdapter.Fill(dataSet, "CatalogTypes");
                    }
                    return dataSet;
                }
            }
        }

        public int CreateCatalogTypes(DataSet dataSet)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(CREATE_TYPES, cnn))
                {
                    cmd.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { SourceColumn = "Id" });
                    cmd.Parameters.Add(new SqlParameter("Type", SqlDbType.VarChar) { SourceColumn = "Type" });
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
                    {
                        dataAdapter.InsertCommand = cmd;
                        return dataAdapter.Update(dataSet, "CatalogTypes");
                    }
                }
            }
        }

        public DataSet GetCatalogBrands()
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(QUERY_BRANDS, cnn))
                {
                    DataSet dataSet = new DataSet();
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                    {
                        dataAdapter.Fill(dataSet, "CatalogBrands");
                    }
                    return dataSet;
                }
            }
        }

        public int CreateCatalogBrands(DataSet dataSet)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(CREATE_BRANDS, cnn))
                {
                    cmd.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { SourceColumn = "Id" });
                    cmd.Parameters.Add(new SqlParameter("Brand", SqlDbType.VarChar) { SourceColumn = "Brand" });
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
                    {
                        dataAdapter.InsertCommand = cmd;
                        return dataAdapter.Update(dataSet, "CatalogBrands");
                    }
                }
            }
        }

        public DataSet GetItemById(int id)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(QUERY_ITEMSBYID, cnn))
                {
                    SqlParameter param = new SqlParameter("id", id);
                    cmd.Parameters.Add(param);
                    DataSet dataSet = new DataSet();
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                    {
                        dataAdapter.Fill(dataSet, "CatalogItems");
                    }
                    return dataSet;
                }
            }
        }

        public DataSet GetItems(int typeId = -1, int brandId = -1, string query = null)
        {
            SqlParameter paramType = null;
            SqlParameter paramBrand = null;
            SqlParameter paramQuery = null;

            string sqlQuery = QUERY_ITEMS;
            string sqlWhere = null;

            if (typeId > -1)
            {
                paramType = new SqlParameter("typeId", typeId);
                sqlWhere = "CatalogTypeId = @typeId";
            }

            if (brandId > -1)
            {
                paramBrand = new SqlParameter("brandId", brandId);
                sqlWhere = sqlWhere == null ? String.Empty : sqlWhere + " AND ";
                sqlWhere += "CatalogBrandId = @brandId";
            }

            if (!String.IsNullOrEmpty(query))
            {
                paramQuery = new SqlParameter("@query", String.Format("%{0}%", query));
                sqlWhere = sqlWhere == null ? String.Empty : sqlWhere + " AND ";
                sqlWhere += "Name LIKE @query";
            }

            sqlQuery = sqlWhere == null ? sqlQuery : sqlQuery + " WHERE " + sqlWhere;

            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    if (paramType != null)
                    {
                        cmd.Parameters.Add(paramType);
                    }
                    if (paramBrand != null)
                    {
                        cmd.Parameters.Add(paramBrand);
                    }
                    if (paramQuery != null)
                    {
                        cmd.Parameters.Add(paramQuery);
                    }
                    DataSet dataSet = new DataSet();
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                    {
                        dataAdapter.Fill(dataSet, "CatalogItems");
                    }
                    return dataSet;
                }
            }
        }

        public int CreateCatalogItems(DataSet dataSet)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(CREATE_ITEMS, cnn))
                {
                    cmd.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { SourceColumn = "Id", Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("Name", SqlDbType.VarChar) { SourceColumn = "Name" });
                    cmd.Parameters.Add(new SqlParameter("Description", SqlDbType.VarChar) { SourceColumn = "Description" });
                    cmd.Parameters.Add(new SqlParameter("Price", SqlDbType.Decimal) { SourceColumn = "Price" });
                    cmd.Parameters.Add(new SqlParameter("CatalogTypeId", SqlDbType.Int) { SourceColumn = "CatalogTypeId" });
                    cmd.Parameters.Add(new SqlParameter("CatalogBrandId", SqlDbType.Int) { SourceColumn = "CatalogBrandId" });
                    cmd.Parameters.Add(new SqlParameter("PictureFileName", SqlDbType.VarChar) { SourceColumn = "PictureFileName" });

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
                    {
                        dataAdapter.InsertCommand = cmd;
                        return dataAdapter.Update(dataSet, "CatalogItems");
                    }
                }
            }
        }

        public int UpdateCatalogItems(DataSet dataSet)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(UPDATE_ITEMS, cnn))
                {
                    cmd.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { SourceColumn = "Id" });
                    cmd.Parameters.Add(new SqlParameter("Name", SqlDbType.VarChar) { SourceColumn = "Name" });
                    cmd.Parameters.Add(new SqlParameter("Description", SqlDbType.VarChar) { SourceColumn = "Description" });
                    cmd.Parameters.Add(new SqlParameter("Price", SqlDbType.Decimal) { SourceColumn = "Price" });
                    cmd.Parameters.Add(new SqlParameter("CatalogTypeId", SqlDbType.Int) { SourceColumn = "CatalogTypeId" });
                    cmd.Parameters.Add(new SqlParameter("CatalogBrandId", SqlDbType.Int) { SourceColumn = "CatalogBrandId" });
                    cmd.Parameters.Add(new SqlParameter("PictureFileName", SqlDbType.VarChar) { SourceColumn = "PictureFileName" });

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
                    {
                        dataAdapter.UpdateCommand = cmd;
                        return dataAdapter.Update(dataSet, "CatalogItems");
                    }
                }
            }
        }

        public int DeleteItem(int id)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(DELETE_ITEM, cnn))
                {
                    SqlParameter param = new SqlParameter("id", id);
                    cmd.Parameters.Add(param);
                    cnn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public DataSet GetImage(int id)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(QUERY_IMAGEBYID, cnn))
                {
                    SqlParameter param = new SqlParameter("id", id);
                    cmd.Parameters.Add(param);
                    DataSet dataSet = new DataSet();
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                    {
                        dataAdapter.Fill(dataSet, "CatalogImages");
                    }
                    return dataSet;
                }
            }
        }

        public int InsertImage(int id, string extension, byte[] image)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(CREATE_IMAGE, cnn))
                {
                    SqlParameter param = new SqlParameter("Id", id);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("ImageType", extension);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("ImageBytes", image);
                    cmd.Parameters.Add(param);
                    cnn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}