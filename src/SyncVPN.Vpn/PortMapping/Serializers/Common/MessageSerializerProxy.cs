/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of SyncVPN.
 *
 * SyncVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * SyncVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with SyncVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using SyncVPN.Vpn.PortMapping.Messages.Common;

namespace SyncVPN.Vpn.PortMapping.Serializers.Common
{
    public class MessageSerializerProxy : IMessageSerializerProxy
    {
        private readonly IMessageSerializerFactory _messageSerializerFactory;

        public MessageSerializerProxy(IMessageSerializerFactory messageSerializerFactory)
        {
            _messageSerializerFactory = messageSerializerFactory;
        }

        public TMessage FromBytes<TMessage>(byte[] serializedMessage)
            where TMessage : MessageBase
        {
            IMessageSerializerOfType<TMessage> serializer = GetSerializer<TMessage>();
            return serializer.FromBytes(serializedMessage);
        }

        private IMessageSerializerOfType<TMessage> GetSerializer<TMessage>()
            where TMessage : MessageBase
        {
            IMessageSerializer serializer = _messageSerializerFactory.Get<TMessage>();
            return (IMessageSerializerOfType<TMessage>)serializer;
        }

        public byte[] ToBytes<TMessage>(TMessage message)
            where TMessage : MessageBase
        {
            IMessageSerializerOfType<TMessage> serializer = GetSerializer<TMessage>();
            return serializer.ToBytes(message);
        }
    }
}