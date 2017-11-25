using System.Collections.Generic;
using System.Linq;
using VirtualBoxApi = VirtualBox;

namespace Wox.Plugin.VirtualBox {
    public class Main : IPlugin {
        private VirtualBoxApi.IVirtualBox _vb;
        private VirtualBoxApi.Session _session;

        /// <summary>
        /// Wox plugin init method
        /// </summary>
        /// <param name="context">Wox context</param>
        public void Init(PluginInitContext context) {
            _vb = new VirtualBoxApi.VirtualBox();
            _session = new VirtualBoxApi.Session();
        }

        /// <summary>
        /// Wox plugin query method
        /// </summary>
        /// <param name="query">Query entered by the user</param>
        /// <returns>List of results to display</returns>
        public List<Result> Query(Query query) {
            var matcher = FuzzyMatcher.Create(query.Search);
            return (from VirtualBoxApi.IMachine machine in _vb.Machines
                    let score = matcher.Evaluate(machine.Name).Score
                    where score > 0
                    select new Result() {
                        Title = machine.Name,
                        SubTitle = machine.State.ToString(),
                        Score = score,
                        Action = _ => {
                            machine.LaunchVMProcess(_session, "gui", string.Empty);
                            return true;
                        }
                    }).ToList();
        }

    }
}
