using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalStack.DAL.Models {
    public enum ItemType {
        File = 0,
        Folder
    }

    public class Item {
        [Key]
        public string Id { get; set; }
        public ItemType Type { get; set; }
        public DateTime DateCreated { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long SizeInBytes { get; set; }

        [ForeignKey(nameof(Parent))]
        public string ParentId { get; set; }
        public Item Parent { get; set; }

        public virtual List<Item> Items { get; set; } = new List<Item>();
    }
}
