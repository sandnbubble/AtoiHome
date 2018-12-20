﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AtoiHomeManager.TextTransferServiceSoap {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="TextTransferServiceSoap.ITextTransfer")]
    public interface ITextTransfer {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransfer/GetRoot", ReplyAction="http://tempuri.org/ITextTransfer/GetRootResponse")]
        string GetRoot();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransfer/GetRoot", ReplyAction="http://tempuri.org/ITextTransfer/GetRootResponse")]
        System.Threading.Tasks.Task<string> GetRootAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransfer/GetData", ReplyAction="http://tempuri.org/ITextTransfer/GetDataResponse")]
        string GetData(string arg);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransfer/GetData", ReplyAction="http://tempuri.org/ITextTransfer/GetDataResponse")]
        System.Threading.Tasks.Task<string> GetDataAsync(string arg);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ITextTransferChannel : AtoiHomeManager.TextTransferServiceSoap.ITextTransfer, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class TextTransferClient : System.ServiceModel.ClientBase<AtoiHomeManager.TextTransferServiceSoap.ITextTransfer>, AtoiHomeManager.TextTransferServiceSoap.ITextTransfer {
        
        public TextTransferClient() {
        }
        
        public TextTransferClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public TextTransferClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TextTransferClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TextTransferClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetRoot() {
            return base.Channel.GetRoot();
        }
        
        public System.Threading.Tasks.Task<string> GetRootAsync() {
            return base.Channel.GetRootAsync();
        }
        
        public string GetData(string arg) {
            return base.Channel.GetData(arg);
        }
        
        public System.Threading.Tasks.Task<string> GetDataAsync(string arg) {
            return base.Channel.GetDataAsync(arg);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="TextTransferServiceSoap.ITextTransferSoap")]
    public interface ITextTransferSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransfer/GetRoot", ReplyAction="http://tempuri.org/ITextTransfer/GetRootResponse")]
        string GetRoot();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransfer/GetRoot", ReplyAction="http://tempuri.org/ITextTransfer/GetRootResponse")]
        System.Threading.Tasks.Task<string> GetRootAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransfer/GetData", ReplyAction="http://tempuri.org/ITextTransfer/GetDataResponse")]
        string GetData(string arg);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransfer/GetData", ReplyAction="http://tempuri.org/ITextTransfer/GetDataResponse")]
        System.Threading.Tasks.Task<string> GetDataAsync(string arg);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransferSoap/DownloadImage", ReplyAction="http://tempuri.org/ITextTransferSoap/DownloadImageResponse")]
        System.IO.Stream DownloadImage(string strFilename);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITextTransferSoap/DownloadImage", ReplyAction="http://tempuri.org/ITextTransferSoap/DownloadImageResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> DownloadImageAsync(string strFilename);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ITextTransferSoapChannel : AtoiHomeManager.TextTransferServiceSoap.ITextTransferSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class TextTransferSoapClient : System.ServiceModel.ClientBase<AtoiHomeManager.TextTransferServiceSoap.ITextTransferSoap>, AtoiHomeManager.TextTransferServiceSoap.ITextTransferSoap {
        
        public TextTransferSoapClient() {
        }
        
        public TextTransferSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public TextTransferSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TextTransferSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TextTransferSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetRoot() {
            return base.Channel.GetRoot();
        }
        
        public System.Threading.Tasks.Task<string> GetRootAsync() {
            return base.Channel.GetRootAsync();
        }
        
        public string GetData(string arg) {
            return base.Channel.GetData(arg);
        }
        
        public System.Threading.Tasks.Task<string> GetDataAsync(string arg) {
            return base.Channel.GetDataAsync(arg);
        }
        
        public System.IO.Stream DownloadImage(string strFilename) {
            return base.Channel.DownloadImage(strFilename);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> DownloadImageAsync(string strFilename) {
            return base.Channel.DownloadImageAsync(strFilename);
        }
    }
}
