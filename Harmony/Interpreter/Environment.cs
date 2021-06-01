using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Interpreter
{
    public class Environment
    {
        public Dictionary<string, Container> Variables;

        public Environment Parent;

        public Environment(Environment parent = null)
        {
            Variables = new();

            if (parent != null)
            {
                // There's probably a better way to do this
                // TODO: find that better way.
                // Linq?
                foreach (var (k, v) in parent.Variables)
                {
                    Variables[k] = v;
                }
            }

            Parent = parent;
        }

        public Environment Extend()
        {
            return new Environment(this);
        }

        public Environment Lookup(string name)
        {
            var scope = this;
            do
            {
                if (scope.Variables.ContainsKey(name))
                    return scope;
                scope = scope.Parent;
            } while (scope != null);
            return null;
        }

        public Container StrictGet(string name)
        {
            if (!Variables.ContainsKey(name))
            {
                throw new Exception($"undefined variable '{name}'");
            }
            return Variables[name];
        }

        public Container Set(string name, Container value)
        {
            var scope = Lookup(name);
            if (scope != null)
            {
                scope.Variables[name] = value;
            } else
            {
                Variables[name] = value;
            }
            return value;
        }

        public Container WideGet(string name)
        {
            var scope = Lookup(name);
            if (scope != null)
                return scope.StrictGet(name);
            throw new Exception($"undefined variable '{name}'");
        }

        public Container Define(string name, Container value)
        {
            return Variables[name] = value;
        }
    }
}
