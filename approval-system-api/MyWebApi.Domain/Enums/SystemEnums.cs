using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWebApi.Domain.Enums
{
    public enum ApprovalStatus
    {
        [Description("尚未陳核")]
        Draft = 1,

        [Description("待簽核")]
        Pending = 2,

        [Description("已核准")]
        Approved = 3,

        [Description("已退回")]
        Rejected = 4,
    }

    public enum UserLevel
    {
        [Description("一般職員")]
        General = 1,

        [Description("經理")]
        Manager = 2,

        [Description("執行長")]
        CEO = 3,
    }
}
