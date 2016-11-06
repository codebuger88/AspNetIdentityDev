using System.Collections.Generic;

namespace AspNetIdentityWeb.Models.ViewModels
{
    public class NodeViewModels
    {
        public List<BackendMenu> Menus { get; set; }
        public List<BackendMenuAction> Actions { get; set; }
    }
}