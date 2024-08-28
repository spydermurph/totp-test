import { useState } from "react";
import { useNavigate } from "react-router-dom";

const Login = () => {
  const [emailAddress, setEmailAddress] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    const blog = { emailAddress, password };

    try{
      const response = await fetch('https://localhost:44363/api/accounts/login', {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(blog)
      });
      if(response.status === 200) {
        let data = await response.json();
        console.log(data.message);
        if(data.isTwoFactor === true) {
          navigate('/twofactor', {state: {email:data.email, provider:data.provider}});
        } else {
          alert("2FA is not set up");
        }
      } else if( response.status === 401) {
        alert("Invalid username or password");
      } else {
        alert("Unknown error");
      }
    } catch(err) {
      console.log(err);
      alert("Could not connect to server");
    }
    /*.then((response) => {
      console.log(response);
      return (response, response.json())
    }).then((response, data) => {
        console.log(response);
        if(response.status === 200) {
          //let data = response.json();
          if(data.isTwoFactor === true){
            navigate('/twofactor', {state: {email:data.email, provider:data.provider}});
          } else {
            alert("2FA is not set up");
          }
          //return response.json();
        } else {
          let data = response.json();
          console.log(data.message);
          //alert(response."");
        }
    })/*.then((data) => {
        console.log(data);
        if(data.isTwoFactor === true){
            //navigate('Twofactor', {state:{email:data.emailAddress}});
            navigate('/twofactor', {state: {email:data.email, provider:data.provider}});
        }
    }).catch((error) => {
      console.log(error);
      alert("Could not connect to server");
    })*/
  }

  return (
    <div className="login">
      <h2>Login</h2>
      <form onSubmit={handleSubmit}>
        <label>Name:</label>
        <input 
          type="text" 
          required 
          value={emailAddress}
          onChange={(e) => setEmailAddress(e.target.value)}
        />
        <label>Password:</label>
        <input
          type="text"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        ></input>
        <button>Login</button>
      </form>
    </div>
  );
}
 
export default Login;