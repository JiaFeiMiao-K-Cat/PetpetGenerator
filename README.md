# Petpet生成器

## 配置

修改`appsettings.json`中`MyOptions`项

+ `BasePath`: 图片文件基路径
+ `MaxSize`: 上传文件大小限制(单位为MiB)
+ `Petpets`: 可用配置项列表, 字典形式, key为名称, value为路径

## Petpet配置项

+ `name`: 名称
+ `type`: 类型, 目前支持`IMG`, `GIF`, `EXTENDED_IMG`, `EXTENDED_GIF`
+ `imageCount`: 图片序列总数
+ `needProcess`: 仅类型为`GIF`时需要, 标明图片序列编号的左右端点(闭区间)
+ `pos`: 叠加图片的坐标信息, 若为`IMG`或`GIF`依次为: 左上角`x`, `y`坐标, 变换后长, 宽<br/>
若为`EXTENDED_IMG`或`EXTENDED_GIF`, 依次为: 图片编号, 图层优先级(为`1`表示头像在顶层, 为`0`表示在底层), 左上角, 右上角, 右下角, 左下角坐标