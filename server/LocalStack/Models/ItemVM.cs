using LocalStack.DAL.Models;
using System;

namespace LocalStack.Models {
    public class ItemVM {
        public string Id { get; set; }
        public ItemType Type { get; set; }
        public DateTime DateCreated { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long SizeInBytes { get; set; } = 0;
    }
}
