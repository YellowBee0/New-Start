# Blueprint V2

这是一个与旧蓝图代码完全隔离的基础框架。Runtime 与 Editor 分属两个程序集，旧的 `GraphAsset`、Presenter 和 View 均未被引用。

## 数据所有权

- `BlueprintAsset` 只持有节点。
- 节点持有端口。
- 一条连接的完整、可多态序列化数据由允许发起该连接的端口持有。
- 目标端口只保存轻量的 incoming 反向索引，方便按端口查询全部连接。
- 节点、端口和连接均使用持久化字符串 ID；Undo 后即使托管实例被 Unity 替换，视图仍可按 ID 对齐。

## 编辑与 Undo

所有写操作都应经过 `BlueprintEditService`。服务会在同一个 Undo 组内记录整个 `BlueprintAsset`，修改双端连接数据，发出 `BlueprintChangeSet`，并标记延迟同步队列。

常规操作使用变更集，只增删或刷新对应 Node/Port/Edge View。Undo/Redo 没有业务命令可重放，`BlueprintUndoCoordinator` 会检测被 Unity 恢复的资产，然后由 `BlueprintGraphProjection.Reconcile` 对稳定 ID 字典做差异同步；它会扫描模型，但不会清空或重建整个 GraphView。

节点、端口或连接的专用字段应分别通过 `ModifyNode`、`ModifyPort`、`ModifyConnection` 修改。不要直接从 View 写模型，也不要在 `Connect`/`Disconnect` View 回调中持久化数据。

## 延迟外部同步

`BlueprintDirtyGraphStore` 以资产 GUID 持久化待同步蓝图，允许继续保留“编辑时只写全局缓存、保存时再更新父蓝图”的策略。

实现 `IBlueprintSideEffectHandler` 并注册到 `BlueprintSideEffectRegistry`：

- 普通编辑由 `OnGraphChanged` 接收增量变更集，可更新内存缓存。
- Undo/Redo 由 `OnGraphRestored` 接收恢复通知，应从当前蓝图重新对齐缓存，而不是假设原来的 Connect/Disconnect 方法会再次执行。

保存流水线完成跨蓝图写回后，再按 GUID 调用 `BlueprintDirtyGraphStore.Clear`。

## 扩展点

- 派生 `BlueprintNodeData`、`BlueprintPortData`、`BlueprintConnectionData` 添加业务数据。
- 端口重写 `CanOwnConnection` 和 `CreateConnectionData` 决定连接所有权及具体连接类型。
- 实现 `IBlueprintViewFactory` 返回业务 Node/Port/Edge View。
- `ModifyNode`、`ModifyPort`、`ModifyConnection` 会递增可序列化且可 Undo 的 `ViewRevision`。Undo/Redo 通过 revision 只刷新真正改变的数据 View；连接集合变化不会修改端口 revision，所以单纯连线变化只操作 Edge。
- 保存前调用 `BlueprintValidator.Validate`，阻止缺失端点、重复 ID、反向索引不一致等损坏数据落盘。

通过 `Window > Blueprint V2 > Editor` 可打开最小测试窗口；工具栏支持创建通用节点、输入/输出端口及校验，连接、删除、移动均可直接测试 Undo/Redo。
