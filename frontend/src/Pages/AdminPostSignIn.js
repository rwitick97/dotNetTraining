import React, { useContext, useEffect, useState } from 'react';
import ReactDOM from 'react-dom/client';
import { UserContext } from './userContext.js';
import { Link } from 'react-router-dom';
import axios from 'axios';

const AdminPostSignIn = () =>{

const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(false)
  useEffect(() => {
    setLoading(true);
    axios.get("https://localhost:7180/api/Users/all").then((response) => {
        console.log(response);
        setUsers(response.data.users);
        setLoading(false)
      })
  }, [])

    return (
        <div>
            <Link  to='/SignUp'>Sign Up new User</Link>
            <Link to='/CreateAccount'>Create new Bank Account</Link>
            {loading ? (<div>Loading ...</div>):(
                <div>
                <h1>Users</h1>
                <table border={1}>
                  <tr>
                    <th>User Id</th>
                    <th>Name</th>
                    <th>Email</th>
                  </tr>
                  {users.map(user => {return (
                    <tr key={user.userid}>
                      <td><Link to={
                        {
                            pathname:'/accounts',
                            userId: `${user.userid}`
                        }
                      }>{user.userid}</Link></td>
                      <td>{user.uname}</td>
                      <td>{user.email}</td>
                    </tr>
                  )})}
                </table>
                </div>
            )}
        </div>
        
    )
}

export default AdminPostSignIn;