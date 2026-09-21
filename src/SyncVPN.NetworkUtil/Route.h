#pragma once

#include <iphlpapi.h>

namespace SyncVpn
{
    namespace NetworkUtil
    {
        namespace Route
        {
            struct IfaceInfo
            {
                ULONG Ipv4Metric;
                IF_INDEX Index;
                IF_LUID Luid;
            };

            bool GetIfaceInfo(UINT index, IfaceInfo& info);
        }
    }
}
