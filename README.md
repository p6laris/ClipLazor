
# ClipLazor: Clipboard API Interop for Blazor

ClipLazor is a library that provides interop for the Clipboard API in Blazor applications, allowing you to easily interact with the clipboard.


![alt text](https://github.com/p6laris/ClipLazor/blob/master/ClipboardLazor.png?raw=true)

[![NuGet](https://img.shields.io/nuget/dt/ClipLazor?logo=nuget)](https://www.nuget.org/packages/ClipLazor)
## Installation
1. Install [ClipLazor](https://www.nuget.org/packages/ClipLazor) with dotnet cli in your Blazor app.

  ```sh
  dotnet add package ClipLazor 
  ```
2. Register the service to the IoC container using `AddClipboard` method:

  ```C#
  using ClipLazor.Extention;

  // Inside your ConfigureServices method
  builder.Service.AddClipboard();
  ```
3.Add this script tag in your root file **_Host.cshtml or App.razor for Blazor Server Apps** and **index.html for Blazor WebAssembly Apps**:
  ```html

  <script src="_framework/blazor.server.js"></script>
  <script src="_content/ClipLazor/clipboard.min.js"></script>
  ```
  
## Usage
After ClipLazor being installed, you can inject it in your razor file using `IClipLazor` interface:

  ```razor
  @using ClipLazor.Components;
  @inject IClipLazor Clipboard
   ```
### Checking Browser Support
You can check if the browser supports the Clipboard API with the `IsClipboardSupported()` asynchronous method:
```C#
bool isSupported = await Clipbaord.IsClipboardSupported();
```

:warning: Most of non-Chromium browsers (Firefox or safari) does not support clipboard api permissions so the `IsClipboardSupported()` and `Clipboard.IsPermitted(PermissionCommand.Write)`, `Clipboard.IsPermitted(PermissionCommand.Read)` will always return **true**. 
Please check this [mdn](https://developer.mozilla.org/en-US/docs/Web/API/Clipboard_API#security_considerations)

### Checking Permissions
The Clipboard API requires user permission to read and write to the clipboard. Use the `IsPermitted()` asynchronous method with the `PermissionCommand` enum to check read or write permission:
```C#
bool isWritePermitted = await Clipboard.IsPermitted(PermissionCommand.Write);
bool isReadPermitted = await Clipboard.IsPermitted(PermissionCommand.Read);
```
### Read And Write Text
1.To copy a text to the clipboard use `WriteTextAsync()` async method and pass the text(:warning: pass the argument by ReadOnlyMemory). The method return **true** if the copy operation is successful; otherwise, **false**:
```C#
string txt = string.Empty;
if(isSupported && txt.Length > 0)
{
    if (isWritePermitted)
    {
        var isCopied = await Clipboard.WriteTextAsync(txt.AsMemory());
        if (isCopied)
        {
            msg = "Text Copied";
        }
        else
        {
            msg = "Couldn't copy the text!.";
        }
    }
}
```
2. To paste a text from the clipboard use 'ReadTextAsync()' async method. if the paste operation was successed the method ruturns the **text string**; otherwise **null**:
```C#
string pastedTxt = string.Empty;
if(isSupported && isReadPermitted)
{
    var pastedText = await Clipboard.ReadTextAsync();
    if (pastedText is not null)
    {
        msg = "Text Pasted";
        pastedTxt = pastedText;
    }
    else
    {
        msg = "Couldn't paste the text!.";
    }
}
```
3. Or you can use the `ClipboardTextAction` component which contain a single html button, to easily perform clipboard actions, such as copying text or cutting content, directly from a text content HTML element or a text source.

**Parameters**
| Parameter       | Type                                     | Description                                                                                  | Default               |
|------------------|------------------------------------------|----------------------------------------------------------------------------------------------|-----------------------|
| `TargetId`       | `string`                                 | The ID of the target element to perform the clipboard action on.                             | `string.Empty`        |
| `Text`           | `string`                                 | The text to copy to the clipboard.                                                          | `string.Empty`        |
| `Action`         | `ClipboardAction`                        | Clipboard action to perform (`Copy` or `Cut`).                                               | `ClipboardAction.Copy` |
| `Class`          | `string`                                 | CSS class to apply to the button.                                                           | `string.Empty`        |
| `Style`          | `string`                                 | Inline styles to apply to the button.                                                       | `string.Empty`        |
| `OnAction`       | `EventCallback<OnTextActionEventArgs>`    | Event triggered when the clipboard action is completed.                                      | `null`                |

**Event Argument**

The `OnTextActionEventArgs` class provides details about the clipboard action:

1. **Action**: The clipboard action performed (Copy or Cut), Copy if `Text` provided.
2. **Text**: The text associated with the action. Empty ("") if failed.
3. **IsSuccess**: A boolean indicating whether the action succeeded.

```razor
<input id="nameInput" />
<ClipboardTextAction TargetId="nameInput" Action="ClipboardAction.Cut" OnAction="HandleAction">Copy</ClipboardTextAction>
@* OR *@
<ClipboardTextAction Text="Hello World!">Say hi to the clipboard</ClipboardTextAction>


@code {
private void HandleAction(OnTextActionEventArgs e){
    string message = string.Empty;

    if (e.IsSuccess)
    {
        var action = e.Action is ClipboardAction.Copy ? "copied" : "cut";
        message = $"Text: {e.Text} has been {action} successfully.";
    }
    else
        message = $"Could not {e.Action.ToString()} to the clipboard.";

 }
}
```
ℹ️ If the Clipboard API is not supported, the `ClipLazor` will automatically falls back to using the `execCommand` method to perform clipboard actions. While `execCommand` is considered deprecated and less reliable, it ensures broader compatibility with older browsers.

### Read And Write Data
:exclamation: When you work with data to copy to the clipboard or paste from it, the `MIME Type` has to be specified. Note that not all MIME types supported by the clipboard api.
#### Common Supported Mime Types:
* **"text/plain"**
* **"text/html"**
* **"text/uri-list"**
* **"image/png"**

1. To copy data to the clipboard use `WriteDataAsync` async method. pass the array buffer of the data and it's associated MIME Type. This method will return **true** if the copy operation is successful; otherwise, **false**:
```C#
 byte[] imgArray = Convert.FromBase64String(imageToCopy);

 if(imgArray.Length > 0 && isWritePermitted)
 {
     var isDataCopied = await Clipboard.WriteDataAsync(imgArray, "image/png");

     if (isDataCopied)
     {
         dmsg = "Image Copied!";
     }
     else
     {
         dmsg = "Failed to copy the image!.";
     }
 }
```
2. To paste the data from the clipboard use `ReadDataAsync` async method and pass the `MIME Type` argument, the method will return **`ReadOnlyMemory<byte>`** of the data; or an **empty one**:
```C#
if (isReadPermitted)
{
    var pastedData = await Clipboard.ReadDataAsync("image/png");
    if (!pastedData.IsEmpty)
    {
        pastedImg = Convert.ToBase64String(pastedData.ToArray());
        dmsg = "Image Pasted";

    }
    else
    {
        dmsg = "Couldn't paste the image";
    }
}
```
:page_facing_up: See the [full example](https://github.com/p6laris/ClipLazor/blob/master/ClipLazor.WASM/Pages/Index.razor) code.

## License
[MIT License](LICENSE.txt)

    
    
 

