using System.Collections.Generic;

namespace AspNetIdentityWeb.Models.ViewModels
{
    public class NodeViewModels
    {
        public List<BackendMenuAction> Actions { get; set; }
        public List<BackendMenuPermission> Permissions { get; set; }
    }
}