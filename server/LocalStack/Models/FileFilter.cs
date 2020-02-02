using LocalStack.DAL.Models;
using System.Linq;

namespace LocalStack.Models {

    public enum SortDirection {
        Asc = 0,
        Desc
    }

    public class FileFilter {
        public string Search { get; set; }
        public string ParentId { get; set; }

        public IQueryable<Item> Filter(IQueryable<Item> items) {
            items = items.Where(x => x.ParentId == ParentId);
            if (!string.IsNullOrEmpty(Search)) {
                string searchLower = Search.ToLower();
                items = items.Where(x => x.Name.ToLower().Contains(searchLower));
            }
            return items.OrderByDescending(x => x.Type)
                .ThenBy(x => x.DateCreated);
        }
    }
}
