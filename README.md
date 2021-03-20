<h2>LODGroup</h2> 
技术原理： https://blog.csdn.net/qq_33700123/article/details/114933721

 <h3>使用教程</h3>
 开发使用版本：Unity2020.2.3<br />
<img src="/DocumentationImages/1.png" width="100%">    
常驻内存： 用法跟自带的一样，不同之处在lod下的模型显示如右边Renderers所示。<br />
流式加载： 只有流式路径有值 “一件流式加载”和“流式加载”两个按钮才会显示。<br />
	  刷新包围盒：刷新LODGroup包围盒<br />
	一键流式加载：将当前LODGroup的所有LOD资源导出到路径并删除常驻内存中的资源<br />
	    流式加载：按钮只针对当前操作的LOD做流式加载。<br />
	加载优先权重：设置加载优先权重<br />

<br />
 <h3>LODGroupConfig</h3>
	Chess/LODGroupConfig打开。<br />
  <img src="/DocumentationImages/2.png" width="100%">   <br /> 
	同时异步加载数量：设置加载队列同时可以有几个在加载<br />
	间隔时长计算屏占比：设置间隔多久计算一次屏占比<br />
	编辑器下启动流式加载：非运行下主相机能够激发流式加载（主要给场景搭建使用）<br />
	
 <h3>流式加载接口</h3> <br />
  <img src="/DocumentationImages/3.png" width="100%">    <br />
	流式加载接口需要继承“IloadAsset”抽象类，继承后的子类实例化了流式加载才生效。可以参考当前已有的脚本“ResourcesLoadAsset”。

