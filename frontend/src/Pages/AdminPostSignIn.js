import React, { useContext, useEffect, useState } from 'react';
import ReactDOM from 'react-dom/client';
import { UserContext } from './userContext.js';
import { Link } from 'react-router-dom';
import axios from 'axios';
import BackButton from '../Component/BackButton.js';

const AdminPostSignIn = () =>{

const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(false)
  const userDetails = useContext(UserContext);
  useEffect(() => {
    setLoading(true);
    const headers = {
      'Content-type': 'application/json',
      'Authorization': `bearer ${userDetails.token}`
  }
    axios.get("https://localhost:7180/api/Users/all", {headers: headers}).then((response) => {
        console.log(response);
        setUsers(response.data.users);
        setLoading(false)
      })
  }, [])

    return (
        <div className="login-container">
            {/* <Link  to='/SignUp'>Sign Up new User</Link>
            <Link to='/CreateAccount'>Create new Bank Account</Link> */}
            {loading ? (<div>Loading ...</div>):(
                <div>
                <h1>Users</h1>
                <BackButton/>
                <div>
                <table  style={{margin: "1rem auto"}} border={1}>
                  <thead>
                  <tr>
                    <th>User Id</th>
                    <th>Name</th>
                    <th>Email</th>
                  </tr>
                  </thead>
                  <tbody>
                  {users.map(user => {return (
                    <tr key={user.userid}>
                      <td><Link to={`/accounts/${user.userid}`}  style={{border: "none"}}>{user.userid}</Link></td>
                      <td>{user.uname}</td>
                      <td>{user.email}</td>
                    </tr>
                  )})}</tbody>
                </table>
                </div>
                </div>
            )}
        </div>
        
    )
}

export default AdminPostSignIn;