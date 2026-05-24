# HclrKits
这是一个更简单，更适合入门的 HybridCLR 演示工程。
## 介绍
若要使用这个工程，你需要了解：
- 一点 Unity C# 基础
- 了解 AssetBundle 工作流程
- 可以使用 Unity 6 的网络环境
项目中包含如下内容，如有需要，你完全可以单独使用它们。
### 代码部分
代码全部位于 ```Assets/Scripts``` 目录，根据用途分在了三个目录中：
- Core 目录：存放无法热更新的代码，包括加载器和日志可视化。
- Editor 目录：编辑器插件目录，存放自动化工具。
- HotUpdates 目录：热更新源代码目录，按程序集区分子目录。
### 运行流程
在场景加载完成后，工程会首先使用 Loader.cs 自动加载对应目录内的所有元数据，程序集和 AssetBundle。完成后，会向 Loader 所在的 gameObject 及其子附件发送 ```OnHotAwake``` 消息。
此时演示用的 HotAwake.cs 执行调用热更新的相关指令。
演示用的热更新程序集 Kits 内的 Hello.cs 将被调用，并且场景内会实例化一个包含它的 prefab 并在日志输出信息。
## 快速开始
- 打开工程，首先前往 ```HybridCLR``` 菜单，选择 ```Install``` 安装 HybridCLR 组件。
- 使用同一菜单下的 ```Generate > All``` 进行初始化。
- 使用 ```HclrKits > Create Bundles``` 打包热更新预制件。
- 使用 ```HclrKits > Copy AOT metadata``` 复制并更名元数据文件。
- 使用 ```HclrKits > Copy Hot Update``` 复制并更名程序集文件。
- 可以运行项目或者开始构建了。
## 补充说明
除了供演示用的热更新程序集和预制件外，本项目并无 HARDCODE 内容，理论上支持多个平台，但可视化日志只响应键盘输入。如果你感到可视化日志字体过小，可使用数字小键盘+-进行调整，最小可调节至您场景内 Logger.cs 配置的最小字号，当然如果想关掉也可以使用起源引擎同款控制台按钮切换显示。