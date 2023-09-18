﻿using ClipLazor.Enums;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipLazor.Components
{
    
    public class ClipLazor : IClipLazor
    {
        private IJSRuntime JSRuntime;

        /// <summary>
        /// Create a new instance of type <see cref="ClipboardLazor"></see>.
        /// </summary>
        /// <param name="JSRuntime">The JSRuntime</param>
        /// <exception cref="ArgumentNullException">Throws if Js runtime was null.</exception>
        public ClipLazor(IJSRuntime JSRuntime)
        {
            this.JSRuntime = JSRuntime ?? throw new ArgumentNullException(nameof(JSRuntime));
        }


        /// <summary>
        /// Get a text and copy it to the clipboard.
        /// </summary>
        /// <param name="text"> The <see cref="System.ReadOnlyMemory{Char}"></see> text to copy to the clipboard</param>
        /// <returns><see cref="System.String"></see> of the response</returns>
        public async ValueTask<bool> WriteTextAsync(ReadOnlyMemory<char> text) => await JSRuntime.InvokeAsync<bool>("copyToClipboard", text.ToString());

        /// <summary>
        /// Read a text from the clipboard.
        /// </summary>
        /// <returns><see cref="System.String"></see> of the readed text.</returns>
        public async ValueTask<string> ReadTextAsync() => await JSRuntime.InvokeAsync<string>("pasteFromClipboard");

        public async ValueTask<bool> IsClipboardSupported() => await JSRuntime.InvokeAsync<bool>("isClipboardSupported");

        public async ValueTask<bool> IsPermitted(PermissionCommand command)
        {
            var response = (command == PermissionCommand.Write) ?
                 await JSRuntime.InvokeAsync<bool>("hasClipboardPermission", "clipboard-write") :
                 await JSRuntime.InvokeAsync<bool>("hasClipboardPermission", "clipboard-read");
            return response;
        }
    }
}
