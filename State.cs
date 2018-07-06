using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AspNetCore_QnA_Bot
{
    public class State : Dictionary<string, object>
{
    private const string NameKey = "name";

    public State()
    {
        this[NameKey] = null;
    }

    public string Name { get; set; }


    }
}
