// src/App.js
import React from 'react';
import Login from './pages/Login'; // 引入你剛寫好的元件

function App() {
  return (
    <div className="App">
      {/* 直接在畫面放上 Login 元件 */}
      <Login />
    </div>
  );
}

export default App;