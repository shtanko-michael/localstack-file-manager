import React from 'react';
import './App.scss';
import FileManager from './components/FileManager/FileManager';
import { provider, toValue } from 'react-ioc';
import appSettings, { AppSettings } from 'appsettings';
import { Layout } from 'antd';
import 'antd/dist/antd.css';
import './App.scss';

const App: React.FC = () => {
  return <Layout>
    <Layout.Content>
      <FileManager />
    </Layout.Content>
  </Layout>;
}
export default provider([AppSettings, toValue(appSettings)])(App);
