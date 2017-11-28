using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VirtualBoxApi = VirtualBox;

namespace Wox.Plugin.VirtualBox {
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
            switch (pascalCased)
            {
                case "FirstOnline": pascalCased = "Online"; break;
                case "LastOnline": pascalCased = "Online"; break;
                case "FirstTransient": pascalCased = "Transient"; break;
                case "LastTransient": pascalCased = "Transient"; break;
            }

            // insert spaces (taken from https://stackoverflow.com/a/5796427)
            return Regex.Replace(pascalCased, "(\\B[A-Z])", " $1");
        }
    }

    /// <summary>
    /// Extension methods for the VirtualBoxApi.IMachine interface.
    /// </summary>
    internal static class MachineExtensions {
        /// <summary>
        /// Returns true if this machine is online, false otherwise.
        /// 
        /// At some point, C# will hopefully start supporting extension properties (see
        /// https://github.com/dotnet/csharplang/issues/192). At that point, it would probably
        /// make sense to turn this method into a property.
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static bool Online(this VirtualBoxApi.IMachine machine) {
            return machine.State >= VirtualBoxApi.MachineState.MachineState_FirstOnline &&
                   machine.State <= VirtualBoxApi.MachineState.MachineState_LastOnline;
        }

        /// <summary>
        /// Returns a Wox.Plugin.Result object corresponding to this machine.
        /// </summary>
        /// <param name="machine">The machine to resultify</param>
        public static Result ToResult(this VirtualBoxApi.IMachine machine) {
            return new Result() {
                Title = machine.Name,
                SubTitle = machine.State.ToDisplayString(),
                IcoPath = Main.IconPath,
                Action = context => {
                    // the machine cannot be launched if it's already online
                    if (machine.Online()) {
                        return false;
                    }

                    var session = new VirtualBoxApi.Session();
                    var progress = machine.LaunchVMProcess(session, "gui", string.Empty);

                    /*
                     * Although everything appears to work fine if we don't unlock the machine after launching,
                     * the VirtualBox SDK reference (http://download.virtualbox.org/virtualbox/SDKRef.pdf) says
                     * that we must unlock it (see the documentation of IMachine::launchVMProcess()). We do this
                     * asynchronously to keep the Wox UI responsive.
                     */
                    Task.Run(() => {
                        progress.WaitForCompletion(10000); // 10s
                        if (progress.ResultCode == 0) {
                            session.UnlockMachine();
                        }
                    });

                    return true;
                }
        };
        }

        /// <summary>
        /// Returns a Wox.Plugin.Result object corresponding to this machine, containing the given score.
        /// </summary>
        /// <param name="machine">The machine to resultify</param>
        /// <param name="score">The score to assign to the result</param>
        public static Result ToScoredResult(this VirtualBoxApi.IMachine machine, int score) {
            var result = machine.ToResult();
            result.Score = score;
            return result;
        }
    }
}
