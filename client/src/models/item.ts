import { observable, computed } from 'mobx'

declare type ItemType = 'file' | 'folder';
declare type FileType = 'image' | 'video' | 'other';
export default class Item {
    constructor(item: Partial<Item>) {
        Object.assign(this, item);
    }
    @observable id: string;
    @observable name: string;
    @observable extension: string;
    @observable dateCreated: Date;
    @observable sizeInBytes: number = 0;
    @observable type: ItemType;

    @computed get fileType(): FileType {
        switch (this.extension) {
            case 'jpg':
            case 'jpeg':
            case 'png':
            case 'gif':
                return 'image';
            case 'wav':
            case 'mp4':
                return 'video';
            default: return 'other';
        }
    }

    static fromJson(json: any): Item {
        return new Item({
            ...json,
            dateCreated: new Date(json.dateCreated)
        });
    }
}