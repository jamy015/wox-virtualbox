using System.Collections.Generic;
using VirtualBoxApi = VirtualBox;

namespace Wox.Plugin.VirtualBox {
    public class Main : IPlugin {
        private VirtualBoxApi.IVirtualBox _vb;

        /// <summary>
        /// Wox plugin init method
        /// </summary>
        /// <param name="context">Wox context</param>
        public void Init(PluginInitContext context) {
            _vb = new VirtualBoxApi.VirtualBox();
        }

        /// <summary>
        /// Wox plugin query method
        /// </summary>
        /// <param name="query">Query entered by the user</param>
        /// <returns>List of results to display</returns>
        public List<Result> Query(Query query) { }

    }
}
