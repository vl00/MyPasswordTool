using Common;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using SilverEx;
using SilverEx.DataVirtualization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyPasswordTool.Models
{
    public partial class DalRepository
    {
        private DbConnInfo DbConnInfo { get; } = IoC.Resolve<DbConnInfo>();

        public Task CheckDB(string db, string key)
        {
            return Task.Factory.StartNew(_f_);

            void _f_()
            {
                var sb = new SqliteConnectionStringBuilder { DataSource = db, Mode = SqliteOpenMode.ReadWrite };
                using (var _con = new SqliteConnection(sb.ConnectionString))
                {
                    try
                    {
                        _con.TryOpen();
                        var k = _con.ExecuteScalar(" select Quote(@k) ", new { k = key });
                        _con.ExecuteScalar($" PRAGMA key={k} ");
                        if (4 < _con.ExecuteScalar<int>("SELECT count(1) FROM sqlite_master WHERE type='table' AND name IN('PaInfoTag','PaTag','PaInfo','PaInfoFile')"))
                            throw new ArgumentNullException("不是painfo数据库");
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_NOTADB)
                    {
                        throw new InvalidOperationException("pwd error or not a db", ex);
                    }
                }
            }
        }

        public async Task NewDB(string db, string key)
        {
            var sb = new SqliteConnectionStringBuilder { DataSource = db, Mode = SqliteOpenMode.ReadWriteCreate };
            using (var _con = new SqliteConnection(sb.ConnectionString))
            {
                await _con.TryOpenAsync().ConfigureAwait(false);
                var k = await _con.ExecuteScalarAsync(" select Quote(@k) ", new { k = key }).ConfigureAwait(false);
                await _con.ExecuteScalarAsync($" PRAGMA key={k} ").ConfigureAwait(false);
                await _con.ExecuteAsync(Consts.str_newdb).ConfigureAwait(false);
            }
        }

        public async Task<T> GetAsync<T>(object id) where T : class
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            return await con.GetAsync<T>(id).ConfigureAwait(false);
        }
    }

    public partial class DalRepository
    {
        public async Task<IEnumerable<PaTag>> GetChildrenPaTags(int pid)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            return await con.QueryAsync<PaTag>("select * from PaTag where PID=@pid order by [Order];", new { pid }).ConfigureAwait(false);
        }

        public async Task<int> AddPaTagAsync(PaTag entity)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            using (var t = con.BeginTransaction())
            {
                await con.InsertAsync(entity, t).ConfigureAwait(false);
                await con.ExecuteAsync("update PaTag set HasChild=1 where ID=@pid", new { pid = entity.PID }, t).ConfigureAwait(false);
                t.Commit();
                return entity.Order = entity.ID;
            }
        }

        public async Task<int> UpdatePaTagAsync(PaTag entity)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            var b = await con.UpdateAsync(entity).ConfigureAwait(false);
            return b ? 1 : 0;
        }

        public async Task UpdatePaTagsAsync(params PaTag[] entities)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            using (var t = con.BeginTransaction())
            {
                await Task.WhenAll(entities.Select(m => con.UpdateAsync(m, t))).ConfigureAwait(false);
                t.Commit();
            }
        }

        public async Task DeletePaTagAsync(int id, int pid)
        {
            if (id <= 0) return;
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            using (var t = con.BeginTransaction())
            {
                await con.ExecuteAsync(@"
delete from PaTag where ID in @id;
update PaTag set HasChild=(case when EXISTS(SELECT 1 from PaTag WHERE PID=@pid) then 1 else 0 end) where ID=@pid;
", new { id = new[] { id }, pid }, t).ConfigureAwait(false);
                t.Commit();
            }
        }
    }

    public partial class DalRepository
    {
        public async Task<bool> TrashDeletedPaInfo()
        {
            var sql = "delete from PaInfo where IsDeleted=1 ";
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            return await con.ExecuteAsync(sql).ConfigureAwait(false) > 0;
        }

        public async Task<int> UpdateSimplePaInfoAsync(SimplePaInfo entity)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            var b = await con.UpdateAsync(entity).ConfigureAwait(false);
            return b ? 1 : 0;
        }

        public async Task<FuncItemsProvider<SimplePaInfo>> GetSimplePainfoByTrash(string order)
        {
            var sql1 = @"
SELECT count(1) FROM PaInfo WHERE IsDeleted=1";
            var sql2 = $@"
SELECT * FROM PaInfo WHERE IsDeleted=1
ORDER BY {(order == Consts.Order.UpdateTime ? "UpdateTime DESC" :
           order == Consts.Order.Title ? $"Title COLLATE {Consts.Order.tccic}" :
           "UpdateTime DESC")}
LIMIT @i,@size";

            await Task.CompletedTask;
            return new FuncItemsProvider<SimplePaInfo>(
                    async (i, size) => await (await DbConnInfo.GetConnAsync()).QueryAsync<SimplePaInfo>(sql2, new { i, size }).ConfigureAwait(false),
                    async () => await (await DbConnInfo.GetConnAsync()).ExecuteScalarAsync<int>(sql1).ConfigureAwait(false)
                );
        }

        public async Task<FuncItemsProvider<SimplePaInfo>> GetSimplePainfoByAll(string order)
        {
            var sql1 = @"
SELECT count(1) FROM PaInfo WHERE IsDeleted=0";
            var sql2 = $@"
SELECT * FROM PaInfo WHERE IsDeleted=0
ORDER BY {(order == Consts.Order.UpdateTime ? "UpdateTime DESC" :
           order == Consts.Order.Title ? $"Title COLLATE {Consts.Order.tccic}" :
           "UpdateTime DESC")}
LIMIT @i,@size";

            await Task.CompletedTask;
            return new FuncItemsProvider<SimplePaInfo>(
                    async (i, size) => await (await DbConnInfo.GetConnAsync()).QueryAsync<SimplePaInfo>(sql2, new { i, size }).ConfigureAwait(false),
                    async () => await (await DbConnInfo.GetConnAsync()).ExecuteScalarAsync<int>(sql1).ConfigureAwait(false)
                );
        }

        public async Task<FuncItemsProvider<SimplePaInfo>> GetSimplePainfoByNoTag(string order)
        {
            var sql1 = @"
SELECT count(1) FROM PaInfo WHERE ID NOT IN (
    SELECT PaInfoID FROM PaInfoTag
) AND IsDeleted=0";
            var sql2 = $@"
SELECT * FROM PaInfo WHERE ID NOT IN (
    SELECT PaInfoID FROM PaInfoTag
) AND IsDeleted=0
ORDER BY {(order == Consts.Order.UpdateTime ? "UpdateTime DESC" :
           order == Consts.Order.Title ? $"Title COLLATE {Consts.Order.tccic}" :
           "UpdateTime DESC")}
LIMIT @i,@size";

            await Task.CompletedTask;
            return new FuncItemsProvider<SimplePaInfo>(
                    async (i, size) => await (await DbConnInfo.GetConnAsync()).QueryAsync<SimplePaInfo>(sql2, new { i, size }).ConfigureAwait(false),
                    async () => await (await DbConnInfo.GetConnAsync()).ExecuteScalarAsync<int>(sql1).ConfigureAwait(false)
                );
        }

        public async Task<FuncItemsProvider<SimplePaInfo>> GetSimplePainfoByTagID(int tagId, string order)
        {
            var sql1 = @"
SELECT count(1) FROM PaInfo pa 
inner join PaInfoTag pat on pat.PaInfoID=pa.ID
WHERE pa.IsDeleted=0 and  pat.TagID=@tagId";
            var sql2 = $@"
SELECT pa.* FROM PaInfo pa 
inner join PaInfoTag pat on pat.PaInfoID=pa.ID
WHERE pa.IsDeleted=0 and  pat.TagID=@tagId
ORDER BY {(order == Consts.Order.UpdateTime ? "pa.UpdateTime DESC" :
           order == Consts.Order.Title ? $"pa.Title COLLATE {Consts.Order.tccic}" :
           "pa.UpdateTime DESC")}
LIMIT @i,@size";

            await Task.CompletedTask;
            return new FuncItemsProvider<SimplePaInfo>(
                    async (i, size) => await (await DbConnInfo.GetConnAsync()).QueryAsync<SimplePaInfo>(sql2, new { tagId, i, size }).ConfigureAwait(false),
                    async () => await (await DbConnInfo.GetConnAsync()).ExecuteScalarAsync<int>(sql1, new { tagId }).ConfigureAwait(false)
                );
        }

        public async Task<FuncItemsProvider<SimplePaInfo>> GetSimplePainfoBySearch(string text, string order)
        {
            var sql1 = @"
SELECT count(1) FROM PaInfo 
WHERE IsDeleted=0 and (Title like @text or TitleDesc like @text)";
            var sql2 = $@"
SELECT * FROM PaInfo
WHERE IsDeleted=0 and (Title like @text or TitleDesc like @text)
ORDER BY {(order == Consts.Order.UpdateTime ? "UpdateTime DESC" :
           order == Consts.Order.Title ? $"Title COLLATE {Consts.Order.tccic}" :
           "UpdateTime DESC")}
LIMIT @i,@size";

            await Task.CompletedTask;
            return new FuncItemsProvider<SimplePaInfo>(
                    async (i, size) => await (await DbConnInfo.GetConnAsync()).QueryAsync<SimplePaInfo>(sql2, new { text = $"%{text}%", i, size }).ConfigureAwait(false),
                    async () => await (await DbConnInfo.GetConnAsync()).ExecuteScalarAsync<int>(sql1, new { text = $"%{text}%" }).ConfigureAwait(false)
                );
        }

        public async Task ClearPaInfoTagByPaInfoID(int id)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            using (var t = con.BeginTransaction())
            {
                await con.ExecuteAsync(@"
update PaInfo set IsDeleted=0 where ID=@id;
DELETE from PaInfoTag WHERE PaInfoID=@id;
", new { id }, t).ConfigureAwait(false);
                t.Commit();
            }
        }

        public async Task AddPaInfoTag(int paInfoID, int tagID)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            using (var t = con.BeginTransaction())
            {
                await con.ExecuteAsync(@"
update PaInfo set IsDeleted=0 where ID=@paInfoID;
INSERT INTO PaInfoTag(TagID,PaInfoID) SELECT @tagID,@paInfoID WHERE @tagID>0 AND NOT EXISTS(
    SELECT 1 FROM PaInfoTag WHERE TagID=@tagID AND PaInfoID=@paInfoID and TagID>0
);
", new { tagID, paInfoID }, t).ConfigureAwait(false);
                t.Commit();
            }
        }

        public async Task DelTruePaInfoAsync(int id)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            using (var t = con.BeginTransaction())
            {
                await con.ExecuteAsync(@"
delete from PaInfo where ID=@id;
delete from PaInfoTag where PaInfoID=@id;
", new { id }, t).ConfigureAwait(false);
                t.Commit();
            }
        }
    }

    public partial class DalRepository
    {
        public async Task<byte[]> GetPaInfoFileData(int painfoID)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            return await con.ExecuteScalarAsync<byte[]>("select File from PaInfoFile where ID=@painfoID", new { painfoID }).ConfigureAwait(false);
        }

        public async Task<PaInfoFile> GetPaInfoFile(int painfoID)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            return await con.QueryFirstOrDefaultAsync<PaInfoFile>("select ID,FileExtname,FileSize from PaInfoFile where ID=@painfoID", new { painfoID }).ConfigureAwait(false);
        }

        public async Task<PaTag[]> FindTagsByPaInfoID(int painfoID)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            return (await con.QueryAsync<PaTag>(@"
select t.* from PaTag t 
inner join PaInfoTag pt on pt.TagID=t.ID
where pt.PaInfoID=@painfoID
order by t.[Order]
", new { painfoID }).ConfigureAwait(false)).ToArray();
        }

        public async Task<int> AddPaInfoAsync(PaInfo pa)
        {
            var con = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            return await con.InsertAsync(pa).ConfigureAwait(false);
        }

        public async Task SavePaInfoAsync(PaInfo pa, PaTag[] tags, PaInfoFile pfm)
        {
            var c = await DbConnInfo.GetConnAsync().ConfigureAwait(false);
            using (var tran = c.BeginTransaction())
            {
                await Task.Run(() =>
                {
                    c.Update(pa, tran);

                    c.Execute("delete from PaInfoTag where PaInfoID=@ID", new { pa.ID }, tran);
                    if (tags != null)
                    {
                        foreach (var t in tags)
                            c.Insert(new PaInfoTag { PaInfoID = pa.ID, TagID = t.ID }, tran);
                    }

                    if (pfm != null)
                    {
                        c.Execute(@"delete from PaInfoFile where ID=@ID", new { pa.ID }, tran);
                        if (pfm.File != null)
                        {
                            //c.Insert(pfm, tran); //只适用于主键是自增的情况
                            c.Execute(@"insert into PaInfoFile(ID,File,FileExtname,FileSize) values(@ID,@File,@FileExtname,@FileSize)", pfm, tran);
                        }
                    }
                }).ConfigureAwait(false);
                tran.Commit();
            }
        }
    }
}
