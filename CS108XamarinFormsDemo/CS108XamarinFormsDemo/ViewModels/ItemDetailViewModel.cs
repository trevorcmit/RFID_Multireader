using System;
using CS108XamarinFormsDemo.Models;

namespace CS108XamarinFormsDemo.ViewModels {
    public class ItemDetailViewModel : BaseViewModel {
        public Item Item { get; set; }
        public ItemDetailViewModel(Item item = null) {
            Title = item?.Text;
            Item = item;
        }
    }
}
