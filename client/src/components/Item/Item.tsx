import React from 'react';
import styles from './Item.module.scss';
import { observer } from 'mobx-react';
import { Icon, Typography, Button } from 'antd';
import { Item } from 'models';
import Humanize from 'humanize-plus';
import { useInstance } from 'react-ioc';
import { AppSettings } from 'appsettings';

type ItemProps = {
  item: Item;
  onSelect: () => void;
  onDownload: () => void;
}
const ItemComponent: React.FC<ItemProps> = ({ item, onSelect, onDownload }) => {
  const settings = useInstance(AppSettings);

  const renderFolder = () => {
    return <>
      <div className={styles.file_manager_item__thumb}>
        <Icon type="folder" />
      </div>
      <div className={styles.file_manager_item__info}>{item.name}</div>
    </>;
  }

  const renderFile = () => {
    return <>
      <div className={styles.file_manager_item__thumb}>
        {
          item.fileType == 'image'
            ? <img src={`${settings.cloudFront}/${item.id}`}></img>
            : <div className="file-icon file-icon-xl" data-type={item.extension}></div>
        }
      </div>
      <div className={styles.file_manager_item__info}>
        <div className={styles.file_manager_item__name}>
          {item.name} <Button onClick={() => onDownload()} icon='download' type='link' size='small'></Button>
        </div>
        <Typography.Text type='secondary'>
          <small>{Humanize.fileSize(item.sizeInBytes)}</small>
        </Typography.Text>
      </div>
    </>;
  }

  if (item.type == 'folder')
    return <div className={styles.file_manager_item} data-type={item.type} onDoubleClick={() => onSelect()}>
      {renderFolder()}
    </div>;
  return <div className={styles.file_manager_item} data-type={item.type}>
    {renderFile()}
  </div>;
}

export default observer(ItemComponent);
