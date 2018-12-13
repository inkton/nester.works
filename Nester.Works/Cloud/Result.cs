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
using Newtonsoft.Json;

namespace Inkton.Nester.Cloud
{
    public class Result<T>
    {
        public Result()
        {   
            ResultCode = -1;
            ResultText = "Uknown Error";
            Data = new Dictionary<string, object>();
        }

        public void SetSuccess(
            string notes = null)
        {
            ResultCode = 0;
            ResultText = "NEST_RESULT_SUCCESS";
            Notes = notes;
        }

        public void SetSuccess(
            string entity,
            T data,
            string notes = null)
        {
            ResultCode = 0;
            ResultText = "NEST_RESULT_SUCCESS";
            Data[entity] = data;
            Notes = notes;
        }

        public void SetSuccess(
            string entity,
            List<T> data,
            string notes = null)
        {
            ResultCode = 0;
            ResultText = "NEST_RESULT_SUCCESS";
            Data[entity] = data;
            Notes = notes;
        }

        public void SetFail(
            string notes = null)
        {
            ResultCode = -200;
            ResultText = "NEST_RESULT_ERROR";
            Notes = notes;
        }

        public void SetFail(
            Exception e,
            string notes = null)
        {
            ResultCode = -200;
            ResultText = "NEST_RESULT_ERROR";
            Notes = notes + " " + e;
        }

        [JsonProperty("notes")]
        public string Notes { get; set; }
        [JsonProperty("result_code")]
        public int ResultCode { get; set; }
        [JsonProperty("result_text")]
        public string ResultText { get; set; }
        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; }
    }
}