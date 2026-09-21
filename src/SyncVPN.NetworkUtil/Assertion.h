#ifndef SYNCVPN_NETWORK_UTIL_ASSERTION_H
#define SYNCVPN_NETWORK_UTIL_ASSERTION_H

#include <Windows.h>

namespace SyncVpn
{
    namespace NetworkUtil
    {
        void assertSuccess(HRESULT result);
    }
}

#endif // SYNCVPN_NETWORKUTIL_ASSERTION_H
