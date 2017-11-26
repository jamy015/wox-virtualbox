﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public List<Result> Query(Query query) {
            var matcher = FuzzyMatcher.Create(query.Search);
            return (from VirtualBoxApi.IMachine machine in _vb.Machines
                    where machine.Accessible > 0 // if a machine is inaccessible, we cannot get its name or state
                    let score = matcher.Evaluate(machine.Name).Score
                    where score > 0
                    select new Result() {
                        Title = machine.Name,
                        SubTitle = machine.State.ToDisplayString(),
                        IcoPath = "icon.png",
                        Score = score,
                        Action = _ => {
                            if (machine.State >= VirtualBoxApi.MachineState.MachineState_FirstOnline &&
                                machine.State <= VirtualBoxApi.MachineState.MachineState_LastOnline) {
                                // the machine is currently being executed
                                return false;
                            }

                            var session = new VirtualBoxApi.Session();
                            var progress = machine.LaunchVMProcess(session, "gui", string.Empty);

                            Task.Run(() => {
                                progress.WaitForCompletion(10000); // 10s
                                if (progress.ResultCode == 0) {
                                    session.UnlockMachine();
                                }
                            });

                            return true;
                        }
                    }).ToList();
        }
    }

    /// <summary>
    /// Extension methods for the VirtualBoxApi.MachineState class.
    /// </summary>
    internal static class MachineStateExtensions {
        /// <summary>
        /// Returns a pretty display string.
        /// </summary>
        public static string ToDisplayString(this VirtualBoxApi.MachineState state)
        {
            // substring call removes "MachineState_" from the beginning
            var pascalCased = state.ToString().Substring(13);

            // handle pseudo-states (see enum MachineState in the VirtualBox SDK Reference)
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (pascalCased) {
                case "FirstOnline": pascalCased = "Online"; break;
                case "LastOnline": pascalCased = "Online"; break;
                case "FirstTransient": pascalCased = "Transient"; break;
                case "LastTransient": pascalCased = "Transient"; break;
            }

            // insert spaces (taken from https://stackoverflow.com/a/5796427)
            return Regex.Replace(pascalCased, "(\\B[A-Z])", " $1");
        }
    }
}
