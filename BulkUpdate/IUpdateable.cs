using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityExtensions
{
    public interface IUpdateable
    {
        T ExecuteScalarBulk<T>(Dictionary<int, string> companyMembers);
    }
}
