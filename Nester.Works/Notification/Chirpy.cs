/*
    Copyright (c) 2017 Inkton.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SlackAPI;

namespace Inkton.Nester.Notification
{
    public class Chirpy
    {
        private SlackTaskClient _client;
        private string _channel;

        public Chirpy(string token, string channel)
        {
            _client = new SlackTaskClient(token);
            _channel = channel;        
        }

        public void Chirp(string message, params object[] args)
        {
            if (args.Length > 0)
            {
                Task.Run(() => _client.PostMessageAsync(
                    _channel, string.Format(message, args)));
            }
            else
            {
                Task.Run(() => _client.PostMessageAsync(
                    _channel, message));
            }
        }

        public async Task<bool> ChirpAsync(string message, params object[] args)
        {
            PostMessageResponse response;

            if (args.Length > 0)
            {
                response = await _client.PostMessageAsync(
                    _channel, string.Format(message, args));
            }
            else
            {
                response = await _client.PostMessageAsync(
                    _channel, message);
            }

            return response.ok;
        }
    }
}