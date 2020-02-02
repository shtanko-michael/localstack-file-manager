import React from 'react';
import styles from './FileManager.module.scss';
import { observer } from 'mobx-react';
import { provider, useInstance } from 'react-ioc';
import FileManagerStore from './FileManagerStore';
import { Button, Upload, Icon, Breadcrumb, Row, Col, Popover, Input } from 'antd';
import { RcFile } from 'antd/lib/upload';
import ItemComponent from '../Item/Item';
import { Item } from 'models';

const FileManager: React.FC = () => {
  const store = useInstance(FileManagerStore);
  const uploadFile = (file: RcFile) => {
    store.uploadFile(file);
    return false;
  }

  const downloadFile = async (file: Item) => {
    const url = await store.getDownloadUrl(file);

    const link = document.createElement('a');
    document.body.appendChild(link);
    link.download = file.name;
    link.href = url;
    link.click();
    document.body.removeChild(link);
  }

  return <div className={styles.file_manager}>
    <Breadcrumb className={styles.file_manager_breadcrumb}>
      <Breadcrumb.Item onClick={() => store.setCurrentFolder(null)}>Home</Breadcrumb.Item>
      {store.breadcrumb.map(x => <Breadcrumb.Item onClick={() => store.setCurrentFolder(x)}>{x.name}</Breadcrumb.Item>)}
    </Breadcrumb>
    <div>
      <Upload beforeUpload={file => uploadFile(file)} showUploadList={false}>
        <Button type='primary'>
          <Icon type="upload" /> Click to upload
        </Button>
      </Upload>
      <Popover
        content={<>
          <Input placeholder={'Folder name'} value={store.folderName} onChange={e => store.setFolderName(e.target.value)} />
          <Button size='small' onClick={() => store.createFolder()} loading={store.folderCreating} style={{ marginTop: '5px' }}>Create</Button>
        </>}
        title={null}
        trigger="click">
        <Button type='primary' style={{ marginLeft: '5px' }}>
          <Icon type="plus" /> Create folder
      </Button>
      </Popover>
    </div>

    <div className={styles.file_manager_grid}>
      <Row type='flex' gutter={[16, 16]}>
        {store.items.map(x => <Col xs={7} md={6} lg={4} xl={4} xxl={2}>
          <ItemComponent item={x}
            onSelect={() => store.setCurrentFolder(x)}
            onDownload={() => downloadFile(x)} />
        </Col>)}
        {store.items.length == 0 && !store.fetching
          ? <span>No content</span>
          : null}
      </Row>
    </div>
  </div>;
}

export default provider(FileManagerStore)(observer(FileManager));
