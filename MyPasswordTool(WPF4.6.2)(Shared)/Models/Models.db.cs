using Dapper.Contrib.Extensions;
using System;

namespace MyPasswordTool.Models
{
    [Table("PaInfo")]
    public partial class PaInfo
    {
        public PaInfo()
        {
            CreateTime = UpdateTime = DateTime.Now;
        }

        [Key]
        public int ID { get; set; }

        public string Title { get; set; }
        public int Type { get; set; }
        public byte[] ICO { get; set; }
        public string TitleDesc { get; set; }

        public string Data { get; set; }
        //public byte[] Files { get; set; }
        //public string FileExtname { get; set; }
        
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public bool IsDeleted { get; set; }
    }

    [Table("PaTag")]
    public partial class PaTag
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public int PID { get; set; }
        public int Order { get; set; }
        public bool HasChild { get; set; }
    }

    [Table("PaInfoTag")]
    public partial class PaInfoTag
    {
        public int TagID { get; set; }
        public int PaInfoID { get; set; }
    }

    [Table("PaInfo")]
    public class SimplePaInfo
    {
        [Key]
        public int ID { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public string TitleDesc { get; set; }
        public byte[] ICO { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    [Table("PaInfoFile")]
    public class PaInfoFile
    {
        public int ID { get; set; }
        public byte[] File { get; set; }
        public string FileExtname { get; set; }
        public double? FileSize { get; set; }
    }
}