using System.Collections.Generic;
using System.Linq;
using VirtualBoxApi = VirtualBox;

namespace Wox.Plugin.VirtualBox {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Main : IPlugin {
        public const string IconPath = "icon.png";

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
        public List<Result> Query(Query query) {
            var machines = from VirtualBoxApi.IMachine machine in _vb.Machines
                where machine.Accessible > 0
                select machine;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (query.Search.Length == 0) {
                return List(machines);
            } else {
                return Search(machines, query);
            }
        }

        /// <summary>
        /// Lists the available VMs.
        /// </summary>
        /// <param name="machines">The available VMs</param>
        /// <returns>The results to display</returns>
        private static List<Result> List(IEnumerable<VirtualBoxApi.IMachine> machines) {
            return (from machine in machines select machine.ToResult()).ToList();
        }

        /// <summary>
        /// Searches in the available VMs.
        /// </summary>
        /// <param name="machines">The available VMs</param>
        /// <param name="query">The query entered by the user</param>
        /// <returns>The results to display</returns>
        private static List<Result> Search(IEnumerable<VirtualBoxApi.IMachine> machines, Query query) {
            var matcher = FuzzyMatcher.Create(query.Search);
            return (from machine in machines
                let score = matcher.Evaluate(machine.Name).Score
                where score > 0
                select machine.ToScoredResult(score)).ToList();
        }
    }
}
