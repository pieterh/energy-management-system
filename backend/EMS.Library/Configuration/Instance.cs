using System;
namespace EMS.Library.Configuration
{
    public record class Instance
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid AdapterId { get; set; }
        public Config Config { get; set; }
    }

    public record Config
    {        
        public string Type { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string EndPoint { get; set; }
    }
}
